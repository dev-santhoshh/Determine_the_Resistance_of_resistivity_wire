using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class DragObject : MonoBehaviour
{
    [Header("User Defined Limits")]
    public Transform minPoint;
    public Transform maxPoint;

    [Header("Snap UI")]
    public GameObject snapButtonImage;
    public float snapRangeCm = 1.5f;       // outer band -> show snap button
    public float zeroDeflectionCm = 0.05f; // "true" 0 deflection tolerance

    [Header("Set Button")]
    public GameObject setButtonImage;      // shown from the start
    public AudioSource audioSource;
    public AudioClip correctBuzzerClip;
    public AudioClip wrongBuzzerClip;
    public float zeroDeflectionVoltage = 0.1f; // treat |voltage| <= this as "0 deflection" on the galvanometer

    [Header("Movement")]
    public float moveSpeed = 0.001f;

    [Header("Event")]
    [Header("Event")]
    public UnityEvent OnPositionChangedOnce;
    public UnityEvent OnCorrectSetPressed;   // 🔥 fires when Set is pressed at correct 0 deflection

    [Header("Control")]
    public bool dragEnabled = true;

    [Header("Galvanometer")]
    public GalvanometerVoltageCalculator galvanometer;
    public S_Rightgap s_Rightgap;
    public ResistanceSystem resistanceBox;

    private Camera cam;
    private Collider col;

    private Vector2 lastInputPos;
    private bool dragging;
    private bool firstDragFrame;

    private Vector3 startPosition;
    private bool eventTriggered;
    private bool movedByInput;

    // Rail data
    private Vector3 dragAxis;
    private Vector3 railOrigin;
    private float minDot;
    private float maxDot;

    void Start()
    {
        cam = Camera.main;
        col = GetComponent<Collider>();

        startPosition = transform.position;

        railOrigin = minPoint.position;
        dragAxis = (maxPoint.position - minPoint.position).normalized;

        minDot = 0f;
        maxDot = Vector3.Distance(minPoint.position, maxPoint.position);

        if (snapButtonImage != null)
            snapButtonImage.SetActive(false);

        // Set button is visible right from the start
        if (setButtonImage != null)
            setButtonImage.SetActive(true);

        // 🔥 Listen for plug removal / resistance change
        if (resistanceBox != null)
            resistanceBox.OnResistanceChanged.AddListener(ResetJockey);
    }


    void Update()
    {
        if (!dragEnabled)
            return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouse();
#endif

#if UNITY_ANDROID || UNITY_IOS
        HandleTouch();
#endif

        if (movedByInput)
            CheckPositionChangeOnce();
    }

    // ---------------- INPUT ----------------

    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
            TryBeginDrag(Input.mousePosition);

        if (Input.GetMouseButton(0) && dragging)
            Drag(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
            EndDrag();
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0) return;

        Touch t = Input.GetTouch(0);

        if (t.phase == TouchPhase.Began)
            TryBeginDrag(t.position);

        if (t.phase == TouchPhase.Moved && dragging)
            Drag(t.position);

        if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            EndDrag();
    }

    void TryBeginDrag(Vector2 screenPos)
    {
        if (!dragEnabled) return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider == col)
        {
            dragging = true;
            lastInputPos = screenPos;
            firstDragFrame = true;
        }
    }

    void EndDrag()
    {
        dragging = false;
        // Do NOT hide snap button here
    }


    // ---------------- DRAG ----------------

    void Drag(Vector2 inputPos)
    {
        if (firstDragFrame)
        {
            lastInputPos = inputPos;
            firstDragFrame = false;
            return;
        }

        Vector2 delta = inputPos - lastInputPos;
        lastInputPos = inputPos;

        float amount = delta.x * moveSpeed;

        if (Mathf.Abs(amount) > 0.00001f)
        {
            movedByInput = true;
            ApplyMovement(amount);
        }
    }

    void ApplyMovement(float amount)
    {
        float dot = Vector3.Dot(transform.position - railOrigin, dragAxis);
        dot += amount;
        dot = Mathf.Clamp(dot, minDot, maxDot);

        transform.position = railOrigin + dragAxis * dot;

        UpdateSnapUI();
    }

    // ---------------- CM CONVERSION ----------------

    float GetJockeyCm()
    {
        float dot = Vector3.Dot(transform.position - railOrigin, dragAxis);
        float t = Mathf.InverseLerp(minDot, maxDot, dot);
        return t * 100f;
    }

    float CmToDot(float cm)
    {
        float t = cm / 100f;
        return Mathf.Lerp(minDot, maxDot, t);
        // If reversed scale → swap minDot & maxDot
    }

    // ---------------- SNAP LOGIC (auto-snap hint button) ----------------

    void UpdateSnapUI()
    {
        if (!dragEnabled || s_Rightgap == null)
        {
            snapButtonImage?.SetActive(false);
            return;
        }

        float jockeyCm = GetJockeyCm();
        float balanceLength = s_Rightgap.balanceLength;

        float diff = jockeyCm - balanceLength;
        float absDiff = Mathf.Abs(diff);

        // Already at (near) 0 deflection -> hide snap hint, no need to snap
        if (absDiff <= zeroDeflectionCm)
        {
            snapButtonImage?.SetActive(false);
            return;
        }

        // Within outer band -> show snap hint
        bool inSnapRange = absDiff <= snapRangeCm;

        snapButtonImage?.SetActive(inSnapRange);
    }


    public void SnapToBalanceLength()
    {
        if (s_Rightgap == null) return;

        float dot = CmToDot(s_Rightgap.balanceLength);
        dot = Mathf.Clamp(dot, minDot, maxDot);

        transform.position = railOrigin + dragAxis * dot;


        dragEnabled = true;
        snapButtonImage?.SetActive(false);

        Debug.Log("✅ Jockey locked at balance length");
    }

    // ---------------- SET BUTTON LOGIC (user's manual claim of 0 deflection) ----------------

    public void OnSetButtonPressed()
    {
        if (galvanometer == null) return;

        float voltage = galvanometer.CurrentVoltage;

        if (Mathf.Abs(voltage) <= zeroDeflectionVoltage)
        {
            // Correct: galvanometer text shows ~0 V -> stop jockey
            EnableDrag(false);
            snapButtonImage?.SetActive(false);
            setButtonImage?.SetActive(false);

            if (audioSource != null && correctBuzzerClip != null)
                audioSource.PlayOneShot(correctBuzzerClip);

            OnCorrectSetPressed?.Invoke();   // 🔥 fire event on correct press

            Debug.Log("✅ Set pressed at 0 deflection — jockey stopped");
        }
        else
        {
            // Wrong: galvanometer text shows non-zero -> don't stop, buzz wrong
            if (audioSource != null && wrongBuzzerClip != null)
                audioSource.PlayOneShot(wrongBuzzerClip);

            Debug.Log("❌ Set pressed while deflection ≠ 0 — buzzer, jockey still free");
        }
    }

    void CheckPositionChangeOnce()
    {
        if (eventTriggered) return;

        if (Vector3.Distance(transform.position, startPosition) > 0.0001f)
        {
            eventTriggered = true;
            OnPositionChangedOnce?.Invoke();
        }
    }

    public void EnableDrag(bool value)
    {
        dragEnabled = value;

        if (!value)
        {
            dragging = false;
            firstDragFrame = false;
        }
    }

    void ResetJockey()
    {
        Debug.Log("🔄 Resistance changed → Re-enabling jockey");

        dragEnabled = true;
        dragging = false;
        movedByInput = false;
        eventTriggered = false;

        // hide snap + set button until user drags again
        snapButtonImage?.SetActive(false);
        setButtonImage?.SetActive(true);
    }

}