using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using static Utils.DebuggerConfig; // Allows properties to be called as if they belong to this object


public class FileLogHandler : BaseLogHandler, IDestroyHandler
{

    private static Dictionary<int, StreamWriter> logWriters = new();
    private const int COMBINED_LOG_KEY = -1; // Must not overlap with index of LogType

    public override void Log(LogData data)
    {
        if (!EnableFileLogging) return;

        EnsureLogFileInitialized();

        var logEntry = ConvertToLogEntry(data, includeStackTrace: true);

        string json = JsonUtility.ToJson(logEntry);

        if (SingleCombinedLog)
        {
            logWriters[COMBINED_LOG_KEY]?.WriteLine(json);
        }
        else
        {
            if (logWriters.TryGetValue((int)data.logType, out var writer))
            {
                writer.WriteLine(json);
            }
        }
    }

    private static void EnsureLogFileInitialized()
    {
        // Early exist if alreeady initialised
        if (logWriters.Count > 0) return;

        // Exit early if disabled file logging
        if (!EnableFileLogging) return;

        string basePath = LogFilePath;
        Directory.CreateDirectory(basePath);

        string fileName = LogFileName;

        if (SingleCombinedLog)
        {
            CreateStreamWriterForLogType(basePath, fileName);
        }
        else
        {
            foreach (LogType type in Enum.GetValues(typeof(LogType)))
            {
                CreateStreamWriterForLogType(basePath, fileName, (int)type);
            }
        }
    }
    private static void CreateStreamWriterForLogType(string basePath, string fileName, int type = COMBINED_LOG_KEY)
    {
        string suffix1 = type switch
        {
            (int)LogType.Log => "_info",
            (int)LogType.Warning => "_warning",
            (int)LogType.Error => "_error",
            (int)LogType.Assert => "_assert",
            (int)LogType.Exception => "_exception",
            _ => ""
        };

        string suffix2 = "";
        switch (StorageStrategy)
        {
            case LogStorage.Monolithic:
                break;
            case LogStorage.CurrentSessionOnly:
                break;

            case LogStorage.Generational: suffix2 = "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"); break;

            case LogStorage.KeepPrevious:
                string newlogFilePath = Path.Combine(basePath, $"{fileName}{suffix1}.jsonl");
                string prevPath = Path.Combine(basePath, $"{fileName}{suffix1}" + "_previous.jsonl");

                if (File.Exists(prevPath))
                    File.Delete(prevPath);
                if (File.Exists(newlogFilePath))
                    File.Move(newlogFilePath, prevPath);

                break;
        }

        bool append = StorageStrategy switch
        {
            LogStorage.CurrentSessionOnly => false,
            LogStorage.KeepPrevious => false,
            _ => true
        };

        string path = Path.Combine(basePath, $"{fileName}{suffix1}{suffix2}.jsonl");
        var writer = new StreamWriter(path, append: append, Encoding.UTF8) { AutoFlush = true };
        logWriters[type] = writer;

    }

    public void OnDestroy()
    {
        foreach (var writer in logWriters.Values)
        {
            writer.Dispose();
        }
        logWriters.Clear();
    }
}