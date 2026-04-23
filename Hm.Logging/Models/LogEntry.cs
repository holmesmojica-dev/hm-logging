using Hm.Logging.Abstractions;

namespace Hm.Logging.Models
{
    /// <summary>
    /// Represents a structured log entry describing a specific event in the system.
    /// </summary>
    /// <remarks>
    /// A <see cref="LogEntry"/> contains the data specific to a single log event,
    /// such as the message, severity level, and optional contextual overrides.
    ///
    /// <para>
    /// When a <see cref="LogContext"/> is set via the logger, its values are applied
    /// as defaults to all log entries.
    /// </para>
    ///
    /// <para>
    /// If a property is defined in both <see cref="LogContext"/> and <see cref="LogEntry"/>,
    /// the value in <see cref="LogEntry"/> takes precedence.
    /// </para>
    ///
    /// <para><b>Override example:</b></para>
    /// <code>
    /// logger.SetContext(new LogContext
    /// {
    ///     Source = "AuthService",
    ///     TraceId = "abc-123"
    /// });
    ///
    /// await logger.LogAsync(new LogEntry
    /// {
    ///     Message = "User login",
    ///     TraceId = "override-999"
    /// });
    /// </code>
    ///
    /// <para>
    /// Result:
    /// </para>
    /// <code>
    /// {
    ///   "message": "User login",
    ///   "source": "AuthService",
    ///   "traceId": "override-999"
    /// }
    /// </code>
    ///
    /// <para><b>Metadata merging:</b></para>
    /// <code>
    /// // Context
    /// Metadata = { ["service"] = "auth" }
    ///
    /// // Entry
    /// Metadata = { ["userId"] = "123" }
    /// </code>
    ///
    /// <para>
    /// Result:
    /// </para>
    /// <code>
    /// {
    ///   "service": "auth",
    ///   "userId": "123"
    /// }
    /// </code>
    ///
    /// <para>
    /// In case of duplicate keys, values from <see cref="LogEntry"/> override
    /// those from <see cref="LogContext"/>.
    /// </para>
    /// </remarks>
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
        /// Values are merged with those from <see cref="LogContext"/> if present.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; init; }
    }
}
