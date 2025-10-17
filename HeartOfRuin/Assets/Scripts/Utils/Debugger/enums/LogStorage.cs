using UnityEngine;

/// <summary>
/// Specifies the storage strategy for log files.
/// </summary>
/// <remarks>This enumeration defines the available strategies for managing log file storage. Each value 
/// represents a different approach to handling log file rotation, retention, and naming conventions.</remarks>
public enum LogStorage
{
    /// <summary>
    /// Represents a monolithic log. Log never rotates and file size grows until max size exceeded.
    /// </summary>
    Monolithic = 0,
    /// <summary>
    /// Appends a date/time stamp to the end of the log file name on application start YYYYMMDDhhmm
    /// </summary>
    Generational = 1,
    /// <summary>
    /// Specifies that the previous value should be retained. renamed to add the suffix _previous. Existing previous log deleted.
    /// </summary>
    KeepPrevious = 2,
    /// <summary>
    /// Clear the log file on startup and only keep that one file.
    /// </summary>
    CurrentSessionOnly = 3,
}
