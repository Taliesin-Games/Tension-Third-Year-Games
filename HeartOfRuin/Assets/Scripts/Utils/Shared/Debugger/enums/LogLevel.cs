using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Specifies the advisory log level for filtering log messages based on their importance.
    /// </summary>
    /// <remarks>The <see cref="LogLevel"/> enumeration is used to control which log messages are processed
    /// and which are ignored,  based on their level of detail and importance.<br /> 
    /// Higher log levels include more detailed information:
    /// <list type="bullet"> 
    /// <item> <description><see cref="None"/> (0): No log messages are
    /// processed. Useful to mute log output if you want to filter a specific class.
    /// </description> </item> 
    /// <item> <description><see cref="Minimal"/> (1): Only the most critical error
    /// information is logged.</description> </item>
    /// <item> <description><see cref="Common"/> (2): Common debug
    /// information is logged.</description> </item>
    /// <item> <description><see cref="Verbose"/> (3): Detailed debug
    /// information, including variable values and function calls, is logged.</description> </item> </list>
    /// Levels higher than <see cref="Verbose"/> can be used for additional granularity if needed. <br /> <br /> 
    /// Individual classes can override the global <see cref="LogLevel"/> by defining their own <see cref="LogLevel"/>, which takes precedence  
    /// if specified. <br /> 
    /// This can be an <see cref="int"/>, must be positive, set negative to disable.<br /> 
    /// Or it can be a <see cref="LogLevel"/> enum. <br />
    /// Both <b>cannot</b> exist on the same class<br /> <br /> 
    /// Not to be confused with <see cref="LogType"/>  eg <see cref="LogType.Error"/> , <see cref="LogType.Warning"/> , <see cref="LogType.Log"/>  etc. </remarks>
    public enum LogLevel
    {
        //Advisory log levels
        // 0 = None, logs with a higher defined level will be ignored.
        // 1 = Minimal - only include the most important error information. Not this is not to be confused with LogType which is acceed through LogError, LogWarning, Log, etc.
        // 2 = Info - Common debug info.
        // 3 = Verbose - Detailed debug info, including variable values and function calls.
        // Higer levels can also be used for additional granularity.
        // Individual classes can ovrride this. If class.LOG_LEVEL (int) exists in class, and is not -1,  it will be used instead of this global level.
        // Classes will also be checked for class.LOG_LEVEL (LogLevel)

        None = 0,
        Minimal = 1,
        Common = 2,
        Verbose = 3
        

    }

}

