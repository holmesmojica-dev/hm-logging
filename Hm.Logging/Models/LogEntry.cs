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
    public record LogEntry
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


        /// <summary>
        /// Creates a new <see cref="LogEntry"/> instance with the specified message.
        /// </summary>
        /// <param name="message">
        /// The message describing the log event. This value cannot be null, empty, or whitespace.
        /// </param>
        /// <param name="level">
        /// The log level indicating the severity of the event. Defaults to <see cref="LogLevel.Information"/>
        /// </param>
        /// <returns>
        /// A new <see cref="LogEntry"/> instance initialized with the provided message,
        /// the default <see cref="LogLevel.Information"/> level, and the current UTC timestamp.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="message"/> is null, empty, or whitespace.
        /// </exception>
        /// <remarks>
        /// This method provides a flexible way to create log entries with default values
        /// and perform basic validation.
        ///
        /// For common scenarios, it is recommended to use helper methods such as
        /// <see cref="Info(string)"/>, <see cref="Warning(string)"/>, or
        /// <see cref="Error(string, Exception)"/>, which simplify log creation for specific levels.
        ///
        /// Additional properties such as <see cref="Source"/>,
        /// <see cref="TraceId"/>, and <see cref="Metadata"/> can be
        /// optionally set after creation or enriched automatically by the logging pipeline.
        /// </remarks>
        public static LogEntry Create(string message, LogLevel level = LogLevel.Information)
        {
            return string.IsNullOrWhiteSpace(message)
                ? throw new ArgumentException("Log message cannot be empty.", nameof(message))
                : new LogEntry
                {
                    Message = message.Trim(),
                    Level = level,
                    Timestamp = DateTime.UtcNow
                };
        }

        /// <summary>
        /// Creates a log entry with Information level.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <returns>A new <see cref="LogEntry"/> instance.</returns>
        public static LogEntry Info(string message)
        {
            return Create(message);
        }

        /// <summary>
        /// Creates a log entry with Warning level.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <returns>A new <see cref="LogEntry"/> instance.</returns>
        public static LogEntry Warning(string message)
        {
            return Create(message) with { Level = LogLevel.Warning };
        }

        /// <summary>
        /// Creates a log entry with Error level and exception details.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="ex">The exception to include.</param>
        /// <returns>A new <see cref="LogEntry"/> instance.</returns>
        public static LogEntry Error(string message, Exception ex)
        {
            return Create(message) with
            {
                Level = LogLevel.Error,
                Exception = ex.ToString()
            };
        }
    }
}
