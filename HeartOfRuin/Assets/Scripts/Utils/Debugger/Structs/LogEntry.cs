using System;
using UnityEngine;

/// <summary>
/// Serializable data container to allow for easy conversion to JSON
/// </summary>
[Serializable]
public struct LogEntry
{
    public string timestamp;
    public string type;
    public int level;
    public string context;
    public string message;
    public string stacktrace;
}
