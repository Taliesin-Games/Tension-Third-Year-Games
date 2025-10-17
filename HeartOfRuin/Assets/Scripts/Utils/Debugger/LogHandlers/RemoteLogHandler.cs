using System;
using UnityEngine;
using static Utils.DebuggerConfig; // Allows properties to be called as if they belong to this object

public class RemoteLogHandler : BaseLogHandler
{
    public async override void Log(LogData data)
    {
        // If settings explicitly disable remote logging, bail out
        if (!EnableRemoteLogging) return;

        string endpoint = RemoteEndpoint;
        if (string.IsNullOrEmpty(endpoint)) return;

        LogEntry logEntry = ConvertToLogEntry(data, includeStackTrace: true);

        string json = JsonUtility.ToJson(logEntry);

        try
        {
            using var request = new UnityEngine.Networking.UnityWebRequest(endpoint, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;       // Keeping short since debugs can potentially be triggered multiple times every frame. System Not feasible on very slow networks.

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await System.Threading.Tasks.Task.Yield();

            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Remote log failed: " + request.error);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Remote log exception: " + ex.Message);
        }
    }
}
