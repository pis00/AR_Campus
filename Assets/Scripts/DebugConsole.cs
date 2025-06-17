using UnityEngine;
using TMPro;

public class DebugConsole : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText;
    private static DebugConsole instance;

    private string logBuffer = "";

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        logBuffer += logString + "\n";

        if (logBuffer.Length > 3000) // limit buffer
            logBuffer = logBuffer.Substring(logBuffer.Length - 3000);

        debugText.text = logBuffer;
    }
}