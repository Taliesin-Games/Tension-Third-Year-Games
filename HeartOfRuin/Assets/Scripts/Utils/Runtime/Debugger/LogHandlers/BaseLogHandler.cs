using System;
using System.Diagnostics;

public abstract class BaseLogHandler : ILogHandler
{
    public abstract void Log(LogData data);

    protected static LogEntry ConvertToLogEntry(LogData data, bool includeStackTrace = false)
    {
        string trace = includeStackTrace ? new StackTrace(2, true).ToString() : null;

        return new LogEntry
        {
            timestamp = DateTime.UtcNow.ToString("o"),
            type = data.logType.ToString(),
            level = data.logLevel,
            context = data.context ? data.context.name : null,
            message = data.message?.ToString(),
            stacktrace = trace
        };
    }

}