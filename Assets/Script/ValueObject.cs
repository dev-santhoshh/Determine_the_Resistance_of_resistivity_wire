using System.Collections;
using UnityEngine;

public class ValueObject : MonoBehaviour
{
    [Header("Settings")]
    public float value;
    public Transform targetPosition;

    [Header("Movement")]
    public float moveDuration = 0.5f;

    [HideInInspector] public Vector3 originalPos;
    [HideInInspector] public Quaternion originalRot;

    private bool selected = false;
    private bool isMoving = false;
    private Coroutine moveRoutine;

    private void Start()
    {
        originalPos = transform.position;
        originalRot = transform.rotation;
    }

    private void OnMouseDown()
    {
        Debug.Log(gameObject.name + " Clicked");

        if (selected || isMoving)
            return;

        if (PuzzleManager.Instance == null)
        {
            Debug.LogError("PuzzleManager Instance is NULL!");
            return;
        }

        PuzzleManager.Instance.SelectObject(this);
    }

    public void MoveToTarget()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(SmoothMove(
            targetPosition.position,
            targetPosition.rotation,
            true
        ));
    }

    public void ResetObject()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(SmoothMove(
            originalPos,
            originalRot,
            false
        ));
    }

    private IEnumerator SmoothMove(Vector3 targetPos, Quaternion targetRot, bool selectState)
    {
        isMoving = true;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / moveDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        selected = selectState;
        isMoving = false;
        moveRoutine = null;
    }

    public void DisableClick()
    {
        enabled = false;
    }

    public bool IsSelected()
    {
        return selected;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}