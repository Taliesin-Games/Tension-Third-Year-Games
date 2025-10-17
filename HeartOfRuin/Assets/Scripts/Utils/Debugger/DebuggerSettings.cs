using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Global configuration for the Debugger utility.
    /// Create one via Assets → Create → Utils → Debugger Settings.
    /// Place it in Assets/Settings for automatic loading.
    /// </summary>
    [CreateAssetMenu(fileName = "DebuggerSettings", menuName = "Utils/Debugger Settings")]
    public class DebuggerSettings : ScriptableObject
    {
        // DebuggerSettings handles creation of the scriptable object for settings saving, for settings management see DebuggerConfig

        [Header("Log File Output")]
        [Tooltip("Enable to output debug to log file.")]
        public bool enableFileLogging = true;
        [Tooltip("If true, all log types are written to a single file. If false, each log type has its own file.")]
        public bool singleCombinedLog = true;

        [Tooltip("Define how long log files are stored." + "\n" + "\n" +
            "Monolithic - Represents a monolithic log. Log never rotates and file size grows until max size exceeded." + "\n" + "\n" +
            "Generational - Appends a date/time stamp to the end of the log file name on application start YYYYMMDDhhmm" + "\n" + "\n" +
            "Keep Previous - Specifies that the previous value should be retained. renamed to add the suffix _previous. Existing previous log deleted." + "\n" + "\n" +
            "Current Session Only - Clear the log file on startup and only keep that one file.")]
        public LogStorage storageStrategy = LogStorage.KeepPrevious;

        [Tooltip("Relative folder path (under Application.persistentDataPath) where logs are written.")]
        public string logFilePath = "Logs";

        [Tooltip("Base name for log files. If not combined, log type suffixes are appended.")]
        public string logFileName = "debug_log";

        [Header("Levels")]
        [Tooltip("Global log level. Can be overridden by per-class LOG_LEVEL.")]
        public LogLevel globalLogLevel = LogLevel.Common;
        [Tooltip("Gives additional granularity if requried beyond Verbose log level.\n\nAccepts any valid int, -1 disables")]
        public int expandedLogLevel = -1;

        [Header("On-Screen Display")]
        [Tooltip("Enable to output debug to screen")]
        public bool enableScreenLogging = true;
        [Tooltip("Default time in seconds for a log message to remain visible on screen.")]
        public float screenShowTime = 10f;
        public int fontSize = 16;

        [Tooltip("Reference resolution for the debug canvas.")]
        public Vector2 screenResolution = new Vector2(1920, 1080);

        [Header("Remote Logging (optional)")]
        [Tooltip("If set, logs can be forwarded to a remote endpoint.")]
        public bool enableRemoteLogging = false;

        [Tooltip("Remote server URL or IP if remote logging is enabled.")]
        public string remoteEndpoint = "http://127.0.0.1:5000/logs";
    }
}
