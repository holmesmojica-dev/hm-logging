using Hm.Logging.Abstractions;

namespace Hm.Logging.Models
{
    /// <summary>
    /// Represents a structured log entry used for logging events across the system.
    /// Designed to be compatible with modern observability and monitoring platforms.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Human-readable message describing the log event.
        /// Should be concise and meaningful.
        /// </summary>
        public string Message { get; init; } = string.Empty;

        /// <summary>
        /// Severity level of the log entry.
        /// Used for filtering and alerting in monitoring systems.
        /// </summary>
        public LogLevel Level { get; init; } = LogLevel.Information;

        /// <summary>
        /// Timestamp of when the log was created in UTC.
        /// </summary>
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Logical source of the log (e.g., service or application name).
        /// Recommended to follow a consistent naming convention.
        /// </summary>
        public string? Source { get; init; }

        /// <summary>
        /// Correlation identifier used to trace requests across distributed systems.
        /// It is recommended to populate this value using an <see cref="ITraceContext"/>
        /// implementation or any distributed tracing mechanism (e.g., Activity).
        /// </summary>
        public string? TraceId { get; init; }

        /// <summary>
        /// Serialized exception details, if any.
        /// It is recommended to use Exception.ToString() to include full stack trace
        /// and inner exception information for better diagnostics.
        /// </summary>
        public string? Exception { get; init; }

        /// <summary>
        /// Additional structured data for the log entry.
        /// Enables advanced filtering and querying in monitoring tools.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; init; }
    }
}
