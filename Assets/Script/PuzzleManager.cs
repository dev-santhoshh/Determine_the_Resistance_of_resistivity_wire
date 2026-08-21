using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [Header("UI")]
    public TMP_Text totalValueText;

    [Header("Feedback Prefabs")]
    public GameObject correctPrefab;
    public GameObject wrongPrefab;

    [Header("Required Total")]
    public float targetValue = 15f;

    [Header("Objects")]
    public List<ValueObject> objects = new List<ValueObject>();

    [Header("Events")]
    public UnityEvent onCorrect;

    private float currentValue = 0f;
    private bool checking = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        totalValueText.text = "Resistance = 0 Ω";
    }

    public void SelectObject(ValueObject obj)
    {
        if (checking)
            return;

        if (obj.IsSelected() || obj.IsMoving())
            return;

        obj.MoveToTarget();

        currentValue += obj.value;
        totalValueText.text = $"Resistance = {currentValue} Ω";
    }

    public void CheckAnswer()
    {
        if (checking)
            return;

        checking = true;

        // Hide both popups first
        correctPrefab.SetActive(false);
        wrongPrefab.SetActive(false);

        if (Mathf.Approximately(currentValue, targetValue))
        {
            correctPrefab.SetActive(true);

            onCorrect?.Invoke();

            foreach (ValueObject obj in objects)
                obj.DisableClick();
        }
        else
        {
            wrongPrefab.SetActive(true);

            foreach (ValueObject obj in objects)
                obj.ResetObject();

            currentValue = 0f;
            totalValueText.text = "Resistance = 0 Ω";

            StartCoroutine(EnableCheckAgain());
        }
    }

    private IEnumerator EnableCheckAgain()
    {
        float maxDuration = 0f;

        foreach (ValueObject obj in objects)
        {
            if (obj.moveDuration > maxDuration)
                maxDuration = obj.moveDuration;
        }

        yield return new WaitForSeconds(maxDuration);

        checking = false;
    }
}