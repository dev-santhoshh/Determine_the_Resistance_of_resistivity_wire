using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class ShowRedLineAtBalanceLength : MonoBehaviour
{
    public S_Rightgap s_Rightgap;
    public Transform meterStart;
    public Transform meterEnd;
    public Transform redLine;
    public float meterLengthCm = 100f;
    public float fixedY = 0.04973028f;

    [Header("Slide Manager Reference")]
    [Tooltip("The StepPlayer that tracks which page/slide is currently active.")]
    public StepPlayer stepPlayer;

    [System.Serializable]
    public class PageEntry
    {
        [Tooltip("This MUST match StepPlayer's currentIndex for this page (i.e. the same index used in StepPlayer.elements).")]
        public int stepIndex;

        [Tooltip("Checkpoint ID to mark completed ONLY when StepPlayer is on this stepIndex.")]
        public int slideID = -1;

        [Tooltip("Any extra event(s) for THIS page only.")]
        public UnityEvent onCorrectForThisPage;
    }

    [Header("Configure one entry PER PAGE that uses this same red-line object")]
    public List<PageEntry> pageEntries;

    public void ShowRedLine()
    {
        if (!s_Rightgap || !meterStart || !meterEnd || !redLine || !stepPlayer) return;

        float t = s_Rightgap.balanceLength / meterLengthCm;

        Vector3 worldPos = Vector3.Lerp(meterStart.position, meterEnd.position, t);
        worldPos.y = fixedY;

        redLine.SetParent(null, true);
        redLine.position = worldPos;
        redLine.gameObject.SetActive(true);

        // 🔥 Ask StepPlayer which page we're currently on
        int currentIndex = stepPlayer.GetCurrentIndex();

        PageEntry activePage = pageEntries.Find(e => e.stepIndex == currentIndex);

        if (activePage == null)
        {
            Debug.LogWarning($"[ShowRedLineAtBalanceLength] No pageEntry configured for stepIndex {currentIndex}.");
            return;
        }

        if (activePage.slideID >= 0 && SlideProgressManager.Instance != null)
            SlideProgressManager.Instance.MarkCompleted(activePage.slideID);

        activePage.onCorrectForThisPage?.Invoke();
    }
}