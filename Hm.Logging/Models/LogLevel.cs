namespace Hm.Logging.Models
{
    /// <summary>
    /// Defines the severity level of a log entry.
    /// Used to categorize logs for filtering, monitoring, and alerting.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Detailed diagnostic information, typically used for debugging.
        /// </summary>
        Trace = 0,

        /// <summary>
        /// Information useful for debugging during development.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// General informational messages about application flow.
        /// </summary>
        Information = 2,

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
}
