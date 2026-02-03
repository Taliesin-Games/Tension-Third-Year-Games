using UnityEngine;

/// <summary>
/// Logs a message to the Unity console with the specified log type and optional context.
/// </summary>
/// <remarks>This method acts as a wrapper around Unity's <see cref="Debug.Log"/>, <see
/// cref="Debug.LogWarning"/>,  <see cref="Debug.LogError"/>, <see cref="Debug.LogAssertion"/>, and <see
/// cref="Debug.LogException"/> methods,  providing a unified interface for logging messages with different log
/// types and optional context. <para> If <paramref name="logType"/> is <see cref="LogType.Exception"/>, the
/// <paramref name="message"/> parameter  must be an <see cref="System.Exception"/> object. If it is not, an
/// error message will be logged instead. </para></remarks>
/// <param name="message">The message to log. This can be any object, and its string representation will be logged.</param>
/// <param name="context">An optional Unity <see cref="Object"/> that provides context for the log message.  If specified, the message
/// will be associated with this object in the Unity Editor.</param>
/// <param name="logType">The type of log message to send. This determines how the message is categorized in the Unity console  (e.g.,
/// <see cref="LogType.Log"/>, <see cref="LogType.Warning"/>, <see cref="LogType.Error"/>).</param>
public class UnityConsoleHandler : BaseLogHandler
{
    public override void Log(LogData data)
    {
        object msg = data.message ?? "<Log Message Missing>";
        var ctx = data.context;

        switch (data.logType)
        {
            case LogType.Log:
                if (ctx) Debug.Log(msg, ctx);
                else     Debug.Log(msg);
                break;
            case LogType.Warning:
                if (ctx) Debug.LogWarning(msg, ctx);
                else     Debug.LogWarning(msg);
                break;
            case LogType.Error:
                if (ctx) Debug.LogError(msg, ctx);
                else     Debug.LogError(msg);
                break;
            case LogType.Assert:
                if (ctx) Debug.LogAssertion(msg, ctx);
                else     Debug.LogAssertion(msg);
                break;
            case LogType.Exception:
                if (msg is System.Exception ex)
                {
                    if (ctx) Debug.LogException(ex, ctx);
                    else     Debug.LogException(ex);
                }
                else
                {
                    string ex_msg = "LogException called with a non-exception message: message follows \n " + data.message;
                    if (ctx) Debug.LogError(ex_msg, data.context);
                    else     Debug.LogError(ex_msg);
                }
                break;
            default:
                string def_msg = "Invalid log type used, faling over to LogError. " + msg;
                if (ctx) Debug.LogError(def_msg, ctx);
                else     Debug.LogError(def_msg);
                break;
        }
    }
}
