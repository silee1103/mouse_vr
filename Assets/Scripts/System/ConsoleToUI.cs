using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ConsoleToUI : MonoBehaviour
{
    public TMP_Text logText;               // ScrollView 안의 Text
    public ScrollRect scrollRect;      // ScrollView 자체
    private Queue<string> logQueue = new Queue<string>();
    private const int maxLines = 200;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string logEntry = "";

        switch (type)
        {
            case LogType.Warning:
                logEntry += "<color=yellow>[Warning]</color> ";
                break;
            case LogType.Error:
            case LogType.Exception:
                logEntry += "<color=red>[Error]</color> ";
                break;
            default:
                logEntry += "<color=white>";
                break;
        }

        logEntry += logString;

        if (type == LogType.Exception)
            logEntry += "\n" + stackTrace;

        if (type == LogType.Log)
            logEntry += "</color>";

        logQueue.Enqueue(logEntry);

        if (logQueue.Count > maxLines)
            logQueue.Dequeue();

        logText.text = string.Join("\n", logQueue.ToArray());

        // 스크롤 자동 하단 고정
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}