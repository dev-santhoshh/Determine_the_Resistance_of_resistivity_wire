using UnityEngine;
using System.Collections;

public class Slide_Plug_Ultimate : MonoBehaviour
{
    [Header("1. Targets")]
    public Transform holeTarget;
    public Transform tableTarget;

    [Header("2. Electron Flow Control")]
    public GameObject electronFlow;


    [Header("Objects to Activate")]
    public GameObject GameObjectToActivate;
    public GameObject GameObjectToActivate2;
    public MultiWireElectronFlow wireFlow;       // Controls moving electrons
    [Header("3. Settings")]
    public float moveSpeed = 5f;

    [Header("Script to Control")]
    public MonoBehaviour scriptToControl;

    [Header("Page Condition")]
    public PlugPageCondition pageCondition;

    public bool isConnected = false;

    void Start()
    {
        if (electronFlow != null)
            electronFlow.SetActive(false);
        if (wireFlow != null)
            wireFlow.SetFlowState(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            CheckClick();
    }

    void CheckClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (isConnected)
                    Disconnect();
                else
                    Connect();

                return;
            }
        }
    }

    // ----------------------------------------------------
    //  STATE HANDLERS (✔ CORRECT SCOPE)
    // ----------------------------------------------------

    void OnPluggedIn()
    {
        if (GameObjectToActivate != null)
            GameObjectToActivate.SetActive(true);

        if (GameObjectToActivate2 != null)
            GameObjectToActivate2.SetActive(true);
        if (wireFlow != null)
            wireFlow.SetFlowState(true);
    }

    void OnPluggedOut()
    {
        if (GameObjectToActivate != null)
            GameObjectToActivate.SetActive(false);

        if (GameObjectToActivate2 != null)
            GameObjectToActivate2.SetActive(false);
    }

    // ----------------------------------------------------
    //  PUBLIC ACTIONS
    // ----------------------------------------------------

    public void Connect()
    {
        Debug.Log("Action: Connecting...");
        isConnected = true;

        pageCondition?.Check(true);

        StopAllCoroutines();

        OnPluggedIn();

        if (wireFlow != null)
            wireFlow.SetFlowState(true);

        StartCoroutine(MoveTo(holeTarget.position, holeTarget.rotation, true));

        if (scriptToControl != null)
            scriptToControl.enabled = true;
    }

    public void Disconnect()
    {
        Debug.Log("Action: Disconnecting...");
        isConnected = false;

        pageCondition?.Check(false);

        StopAllCoroutines();

        OnPluggedOut();

        if (wireFlow != null)
            wireFlow.SetFlowState(false);

        if (electronFlow != null)
            electronFlow.SetActive(false);

        StartCoroutine(MoveTo(tableTarget.position, tableTarget.rotation, false));

        if (scriptToControl != null)
            scriptToControl.enabled = false;
    }

    // ----------------------------------------------------
    //  MOVEMENT
    // ----------------------------------------------------

    IEnumerator MoveTo(Vector3 targetPos, Quaternion targetRot, bool turnOnFlow)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.001f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        if (turnOnFlow && electronFlow != null)
            electronFlow.SetActive(true);
    }
}
