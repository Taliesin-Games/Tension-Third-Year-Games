using UnityEngine;
using Utils;

public class ObjectExample : MonoBehaviour
{
    // LOG_LEVEL allows you to define a log level for this class specifically, this overrides the global settings.
    // This is optional, without it the global settings or defaults will be used
    // This can be an int or a LogLevel enum.
    // IMPORTANT: Do NOT use both, this will conflict.
    
    const int LOG_LEVEL = 2;
    //const LogLevel LOG_LEVEL = LogLevel.Common; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Used just like Debug.Log() with some additional options.
        Debugger.Log("Please Visit https://github.com/bigmonmulgrew/UnityRemoteLogServer for the server side app"); // Defaults to log level 2
        // Debug.Log("Hello");

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Debugger.Log("Hello on press");

        if (Input.GetKeyDown(KeyCode.Alpha2))
            Debugger.LogWarning("Warning on press");

        if (Input.GetKeyDown(KeyCode.Alpha3))
            Debugger.LogError("Error on press");

        if (Input.GetKeyDown(KeyCode.Alpha4))
            Debugger.LogAssertion("Assert on press");

        // Log a system exception to Debugger.LogException
        // Use a generated template
        if (Input.GetKeyDown(KeyCode.Alpha5))
            Debugger.LogException(new System.Exception("This is a debugging test exception"));
            

    }
}
