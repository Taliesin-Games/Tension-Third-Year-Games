using System.IO;
using UnityEngine;
using Unity.Mathematics;

namespace Utils
{
    public static class DebuggerConfig
    {
        // Add custom getter and setter so if its accessed and doesnt exist it is created
        private static DebuggerSettings settings;

        static readonly Vector2 DEFAULT_SCREEN_RESOLUTION = new(1920, 1080);   // Vectors are classes so cannot be constant, make readonly for assignment only during construction.
        
        #region Properties
        // Advisory log levels
        // 0 = None, logs with a higher defined level will be ignored.
        // 1 = Minimal - only include the most important error information. Not this is not to be confused with LogType which is acceed through LogError, LogWarning, Log, etc.
        // 2 = Common - Common debug info.
        // 3 = Verbose - Detailed debug info, including variable values and function calls.
        // Higer levels can also be used for additional granularity.
        // Individual classes can ovrride this. If class.LOG_LEVEL exists in class, and is not -1,  it will be used instead of this global level.
        public static int GlobalLogLevel => settings != null ? (int)settings.globalLogLevel : 2;
        public static int ExpandedLogLevel => settings != null ? settings.expandedLogLevel : -1; // Do not make the default positive it will conflict with mode switching.
        public static int EffectiveGlobalLogLevel => math.max(GlobalLogLevel, ExpandedLogLevel);
        public static bool EnableFileLogging => settings?.enableFileLogging ?? true;
        public static bool EnableScreenLogging => settings?.enableScreenLogging ?? true;
        public static bool EnableRemoteLogging => settings?.enableRemoteLogging ?? false;
        public static LogStorage StorageStrategy => settings != null ? settings.storageStrategy : LogStorage.KeepPrevious;
        public static bool SingleCombinedLog => settings?.singleCombinedLog ?? true;
        public static string LogFilePath => settings != null
                ? Path.Combine(Application.persistentDataPath, settings.logFilePath)
                : Path.Combine(Application.persistentDataPath, "Logs");
        public static string LogFileName => settings?.logFileName ?? "Debug_Log";
        public static float ScreenShowTime => settings?.screenShowTime ?? 10f;
        public static int FontSize => settings?.fontSize ?? 16;
        public static Vector2 ScreenResolution => settings?.screenResolution ?? DEFAULT_SCREEN_RESOLUTION;  // Using a predefined default here to save running the Vector2 Constructor every time
        public static string RemoteEndpoint => settings?.remoteEndpoint ?? "http://127.0.0.1:5000/logs";
        #endregion
        static DebuggerConfig()
        {
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets(DebuggerSettingsInitializer.SETTINGS_FILE_NAME + " t:Utils.DebuggerSettings");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                settings = UnityEditor.AssetDatabase.LoadAssetAtPath<DebuggerSettings>(path);
            }
#else
            // Build: load from Resources
            settings = Resources.Load<DebuggerSettings>(DebuggerSettingsInitializer.RESOURCES_FILE_NAME);
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Preinitialise()
        {
            if (GlobalLogLevel > 0) return; // Acessing a static on runtime initialize forces running the constructor and prevents thread safety issues, doing this is important.
        }
    }
}

