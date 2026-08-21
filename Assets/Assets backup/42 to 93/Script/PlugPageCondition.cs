using UnityEngine;
using UnityEngine.Events;

public class PlugPageCondition : MonoBehaviour
{
    public enum RequiredState
    {
        PluggedIn,
        PluggedOut
    }

    [System.Serializable]
    public class PageCondition
    {
        [Header("Page")]
        public int pageIndex;

        [Header("Required Plug State")]
        public RequiredState requiredState;

        [Header("Called if condition is correct")]
        public UnityEvent onCorrect;

        [Header("Called if condition is wrong (Optional)")]
        public UnityEvent onWrong;
    }

    [Header("References")]
    public StepPlayer stepPlayer;

    [Header("Conditions")]
    public PageCondition[] conditions;

    public void Check(bool isPluggedIn)
    {
        if (stepPlayer == null)
            return;

        int currentPage = stepPlayer.GetCurrentIndex();

        foreach (var condition in conditions)
        {
            if (condition.pageIndex != currentPage)
                continue;

            bool success =
                (condition.requiredState == RequiredState.PluggedIn && isPluggedIn) ||
                (condition.requiredState == RequiredState.PluggedOut && !isPluggedIn);

            if (success)
            {
                Debug.Log($"[PlugPageCondition] Page {currentPage} Correct");
                condition.onCorrect?.Invoke();
            }
            else
            {
                Debug.Log($"[PlugPageCondition] Page {currentPage} Wrong");
                condition.onWrong?.Invoke();
            }

            return;
        }

        Debug.Log($"[PlugPageCondition] No condition configured for page {currentPage}");
    }
}