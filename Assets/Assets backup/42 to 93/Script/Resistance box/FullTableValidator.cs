//using UnityEngine;
//using UnityEngine.Events;
//using TMPro;

//public class FullTableValidator : MonoBehaviour
//{
//    [System.Serializable]
//    public class Row
//    {
//        public TMP_InputField R;
//        public TMP_InputField L;
//        public TMP_InputField S;

//        public GameObject correctImage;

//        [HideInInspector] public bool isDone;
//        public UnityEvent onRowCorrect;
//    }

//    [Header("Rows (0-4 = S1, 5-9 = S2)")]
//    public Row[] rows = new Row[10];

//    [Header("Mean")]
//    public TMP_InputField meanField;
//    public UnityEvent onMeanCorrect;

//    [Header("Final Result")]
//    public TMP_InputField finalField;
//    public UnityEvent onFinalCorrect;

//    [Header("Group Events")]
//    public UnityEvent onS1Complete;
//    public UnityEvent onS2Complete;
//    public UnityEvent onAllRowsComplete;

//    private int s1Count = 0;
//    private int s2Count = 0;
//    private bool meanUnlocked = false;

//    // =========================
//    // ✅ CHECK BUTTON
//    // =========================
//    public void ValidateAll()
//    {
//        for (int i = 0; i < rows.Length; i++)
//        {
//            ValidateRow(i);
//        }
//    }

//    void ValidateRow(int index)
//    {
//        Row row = rows[index];

//        if (row.isDone) return;

//        if (string.IsNullOrWhiteSpace(row.R.text) ||
//            string.IsNullOrWhiteSpace(row.L.text) ||
//            string.IsNullOrWhiteSpace(row.S.text))
//            return;

//        float r, l, s;

//        if (!float.TryParse(row.R.text, out r)) return;
//        if (!float.TryParse(row.L.text, out l)) return;
//        if (!float.TryParse(row.S.text, out s)) return;

//        // =========================
//        // 🔥 FORMULA VALIDATION
//        // CHANGE THIS RULE IF NEEDED
//        // =========================
//        bool isCorrect = Mathf.Approximately(s, r + l);

//        if (isCorrect)
//        {
//            CompleteRow(index);
//        }
//    }

//    // =========================
//    // 🔥 COMMON COMPLETE LOGIC
//    // =========================
//    void CompleteRow(int index)
//    {
//        Row row = rows[index];

//        if (row.isDone) return;

//        row.isDone = true;

//        // 🔒 Lock fields
//        row.R.interactable = false;
//        row.L.interactable = false;
//        row.S.interactable = false;

//        // ✅ Show correct image
//        if (row.correctImage != null)
//            row.correctImage.SetActive(true);

//        // ✅ Fire event (optional extra visuals)
//        row.onRowCorrect?.Invoke();

//        // 📊 Counting
//        if (index < 5)
//        {
//            s1Count++;
//            if (s1Count == 5)
//                onS1Complete?.Invoke();
//        }
//        else
//        {
//            s2Count++;
//            if (s2Count == 5)
//                onS2Complete?.Invoke();
//        }

//        CheckAllRowsComplete();
//    }

//    void CheckAllRowsComplete()
//    {
//        if (!meanUnlocked && s1Count == 5 && s2Count == 5)
//        {
//            meanUnlocked = true;
//            onAllRowsComplete?.Invoke();
//        }
//    }

//    // =========================
//    // 📊 MEAN VALIDATION (DYNAMIC)
//    // Example: mean of all S values
//    // =========================
//    public void ValidateMean()
//    {
//        if (!meanUnlocked) return;

//        float inputMean;
//        if (!float.TryParse(meanField.text, out inputMean)) return;

//        float total = 0f;
//        int count = 0;

//        foreach (var row in rows)
//        {
//            float s;
//            if (float.TryParse(row.S.text, out s))
//            {
//                total += s;
//                count++;
//            }
//        }

//        float actualMean = total / count;

//        if (Mathf.Approximately(inputMean, actualMean))
//        {
//            onMeanCorrect?.Invoke();
//        }
//    }

//    // =========================
//    // 🏁 FINAL VALIDATION (Example)
//    // =========================
//    public void ValidateFinal()
//    {
//        float input;
//        if (!float.TryParse(finalField.text, out input)) return;

//        // Example: sum of all S
//        float total = 0f;

//        foreach (var row in rows)
//        {
//            float s;
//            if (float.TryParse(row.S.text, out s))
//                total += s;
//        }

//        if (Mathf.Approximately(input, total))
//        {
//            onFinalCorrect?.Invoke();
//        }
//    }

//    // =========================
//    // 🤖 AUTO FILL (SMART)
//    // =========================
//    public void AutoFillRow(int index)
//    {
//        if (index < 0 || index >= rows.Length) return;

//        Row row = rows[index];

//        if (row.isDone) return;

//        float r = Random.Range(1, 10);
//        float l = Random.Range(1, 10);
//        float s = r + l; // MUST FOLLOW SAME RULE

//        row.R.text = r.ToString();
//        row.L.text = l.ToString();
//        row.S.text = s.ToString();

//        CompleteRow(index);
//    }

//    public void AutoFillAll()
//    {
//        for (int i = 0; i < rows.Length; i++)
//        {
//            AutoFillRow(i);
//        }
//    }
//}