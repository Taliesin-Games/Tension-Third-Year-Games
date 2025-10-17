using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using static Utils.DebuggerConfig; // Allows properties to be called as if they belong to this object

using Debug = UnityEngine.Debug;


namespace Utils
{
    public class Debugger : MonoBehaviour
    {
        
        #region Statics
        public static Debugger Instance;
        //static DebuggerSettings settings;
        static readonly Dictionary<Type, int> logLevelCache = new();
     
        private static readonly List<ILogHandler> handlers = new()
        {
            new UnityConsoleHandler(),
            new FileLogHandler(),
            new ScreenLogHandler(),
            new RemoteLogHandler()
        };
        #endregion

        /// <summary>
        /// Initializes the debugger at runtime after the scene has loaded.
        /// </summary>
        /// <remarks>This method is automatically invoked by Unity after the scene has loaded, as
        /// specified by the  <see cref="RuntimeInitializeOnLoadMethodAttribute"/> with the <see
        /// cref="RuntimeInitializeLoadType.AfterSceneLoad"/> parameter. It ensures that a singleton instance of the
        /// <c>Debugger</c> class is created and persists across scene loads.  The method creates a GameObject named
        /// "Utilities: Debugger" with a <c>Debugger</c> component attached,  and marks it as non-destroyable on scene
        /// loads. Additionally, it sets up a child Canvas GameObject configured  for screen space overlay rendering,
        /// with a resolution scaling mode, default of 1080p.</remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void RuntimeInit()
        {
            if (Instance != null) return;

            // Create singleton debugger object
            GameObject debuggerGameObject = new GameObject("Utilities: Debugger");
            Instance = debuggerGameObject.AddComponent<Debugger>();
            DontDestroyOnLoad(debuggerGameObject);

            foreach (ILogHandler handler in handlers)
            {
                if (handler is ICanvasHandler hasCanvas)
                    hasCanvas.InitCanvas(debuggerGameObject.transform);
            }

        }
        private void Awake()
        {
            EnforceSingleton();
        }
        /// <summary>
        /// Ensures that only one instance of the class exists in the scene.
        /// </summary>
        /// <remarks>If another instance of the class already exists, the current instance is destroyed. 
        /// Otherwise, the current instance is set as the singleton instance and marked to persist  across scene
        /// loads.</remarks>
        private void EnforceSingleton()
        {

            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }
        private void Update()
        {
            RunHandlerUpdates();
        }

        private static void RunHandlerUpdates()
        {
            foreach (ILogHandler handler in handlers)
            {
                if (handler is IUpdatableHandler updatable)
                    updatable.OnUpdate();
            }
        }

        public static void Log(object message, UnityEngine.Object  context = null, LogType logType = LogType.Log, int logLevel = 2)
        {
            if (Instance == null) return;

            if (!ShouldLog(logLevel)) return;

            var data = new LogData
            {
                message = message,
                context = context,
                logType = logType,
                logLevel = logLevel
            };

            foreach (var handler in handlers)
                handler.Log(data);

            //Instance.SendUnityDebugMessage(message, context, logType);
            //Instance.SendDebugInfoToFile(message, context, logType, logLevel);
            //Instance.SendDebugInfoToScreen(message, context, logType);
            //Instance.SendDebugInfoToRemote(message, context, logType, logLevel);
        }
        public static void LogWarning(object message, UnityEngine.Object context = null, int logLevel = 2)          => Log(message, context, LogType.Warning, logLevel);
        public static void LogError(object message, UnityEngine.Object context = null, int logLevel = 2)            => Log(message, context, LogType.Error, logLevel);
        public static void LogException(Exception exception, UnityEngine.Object context = null, int logLevel = 2)   => Log(exception, context, LogType.Exception, logLevel);
        public static void LogAssertion(object message, UnityEngine.Object context = null, int logLevel = 2)        => Log(message, context, LogType.Assert, logLevel);
        #region Overloads
        public static void Log(object message)                  => Log(message, null, LogType.Log, (int)LogLevel.Common);
        public static void LogWarning(object message)           => Log(message, null, LogType.Warning, (int)LogLevel.Common);
        public static void LogError(object message)             => Log(message, null, LogType.Error, (int)LogLevel.Common);
        public static void LogException(Exception exception)    => Log(exception, null, LogType.Exception, (int)LogLevel.Common);
        public static void LogAssertion(object message)         => Log(message, null, LogType.Assert, (int)LogLevel.Common);

        public static void Log(object message, UnityEngine.Object context)               => Log(message, context, LogType.Log, (int)LogLevel.Common);
        public static void LogWarning(object message, UnityEngine.Object context)        => Log(message, context, LogType.Warning, (int)LogLevel.Common);
        public static void LogError(object message, UnityEngine.Object context)          => Log(message, context, LogType.Error, (int)LogLevel.Common);
        public static void LogException(Exception exception, UnityEngine.Object context) => Log(exception, context, LogType.Exception, (int)LogLevel.Common);
        public static void LogAssertion(object message, UnityEngine.Object context)      => Log(message, context, LogType.Assert, (int)LogLevel.Common);

        public static void Log(object message, int logLevel) => Log(message, null, LogType.Log, logLevel);
        public static void LogWarning(object message, int logLevel) => Log(message, null, LogType.Warning, logLevel);
        public static void LogError(object message, int logLevel) => Log(message, null, LogType.Error, logLevel);
        public static void LogException(Exception exception, int logLevel) => Log(exception, null, LogType.Exception, logLevel);
        public static void LogAssertion(object message, int logLevel) => Log(message, null, LogType.Assert, logLevel);

        public static void Log<T>(T obj, int logLevel = (int)LogLevel.Common) => Log(obj?.ToString(), null, LogType.Log, logLevel);
        public static void LogWarning<T>(T obj, int logLevel = (int)LogLevel.Common) => Log(obj?.ToString(), null, LogType.Warning, logLevel);
        public static void LogError<T>(T obj, int logLevel = (int)LogLevel.Common) => Log(obj?.ToString(), null, LogType.Error, logLevel);
        public static void LogAssertion<T>(T obj, int logLevel = (int)LogLevel.Common) => Log(obj?.ToString(), null, LogType.Assert, logLevel);
        public static void LogException<T>(T obj, int logLevel = (int)LogLevel.Common) => Log(obj?.ToString(), null, LogType.Exception, logLevel);

        public static void LogException(string message, Exception ex, UnityEngine.Object context = null) => Log(message + "\n" + ex, context, LogType.Exception, (int)LogLevel.Common);
        #endregion
        private static bool ShouldLog(int messageLogLevel)
        {
            // Determine caller type (skip this method and Log())
            var frame = new System.Diagnostics.StackTrace().GetFrame(2);
            Type callerType = frame?.GetMethod()?.DeclaringType;

            if (callerType == null)
            {
                Debug.LogError("Unable to determine class of calling function.");
                return true; // fallback, allow log
            }

            // Look up cached value
            if (!logLevelCache.TryGetValue(callerType, out int effectiveLevel))
            {
                effectiveLevel = GetClassLogLevel(callerType);
                logLevelCache[callerType] = effectiveLevel;
            }

            return messageLogLevel <= effectiveLevel;
        }
        private static int GetClassLogLevel(Type type)
        {
            int level = EffectiveGlobalLogLevel;

            // Look for const/static fields named LOG_LEVEL
            var field = type.GetField("LOG_LEVEL",
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Static | BindingFlags.FlattenHierarchy);

            if (field != null)
            {
                if (field.FieldType == typeof(int))
                    level = (int)field.GetRawConstantValue();
                else if (field.FieldType == typeof(LogLevel))
                    level = (int)(LogLevel)field.GetRawConstantValue();
                else
                    Debug.LogWarning("Field called LOG_LEVEL of incorrect type found, please use int or LogLevel, or if an unrelated variable rename to prevent reflection issues.");
            }

            return level;
        }

        private void OnDestroy()
        {
            foreach (ILogHandler handler in handlers)
            {
                if (handler is IDestroyHandler destroyable)
                    destroyable.OnDestroy();
            }
        }
    }
}


