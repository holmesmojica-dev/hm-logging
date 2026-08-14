namespace Hm.Logging.Enums;

/// <summary>
/// Defines the severity level of a log entry.
/// Used to categorize logs for filtering, monitoring, and alerting.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// General informational messages about application flow.
    /// </summary>
    Information = 0,

    /// <summary>
    /// Detailed diagnostic information, typically used for debugging.
    /// </summary>
    Trace = 1,

    /// <summary>
    /// Developer-focused diagnostic information useful during troubleshooting and debugging.
    /// </summary>
    Debug = 2,

    /// <summary>
    /// Indicates a potential issue or unexpected situation.
    /// </summary>
    Warning = 3,

    /// <summary>
    /// Indicates a failure that occurred during execution.
    /// </summary>
    Error = 4,

    /// <summary>
    /// Indicates a critical failure that may stop the application.
    /// </summary>
    Critical = 5
}
