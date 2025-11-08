using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugUILogger : MonoBehaviour
{
    public static DebugUILogger Instance;

    [Header("UI Reference")]
    public TMP_Text debugText;
    [Header("Settings")]
    public int maxLines = 8;

    private Queue<string> logQueue = new Queue<string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Subscribe to Unity's log callback
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // ✅ Show only logs that come from GameManager.cs
        if (type == LogType.Log || type == LogType.Warning)
        {
            if (stackTrace.Contains("GameManager"))
            {
                AddLine(logString);
            }
        }
    }

    public void AddLine(string message)
    {
        // 🧾 Plain white text only
        if (logQueue.Count >= maxLines)
            logQueue.Dequeue();

        logQueue.Enqueue(message);
        RefreshText();
    }

    private void RefreshText()
    {
        debugText.text = string.Join("\n", logQueue.ToArray());
    }

    // Optional manual call (if you ever want to force log something)
    public void Log(string msg)
    {
        AddLine(msg);
    }
}
