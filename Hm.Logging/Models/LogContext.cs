namespace Hm.Logging.Models
{
    /// <summary>
    /// Represents contextual information shared across multiple log entries
    /// within the same execution flow (e.g., a request, background job, or transaction).
    /// 
    /// This avoids repeating common values such as TraceId, Source, or shared metadata
    /// in every log entry.
    /// 
    /// Values defined in the context act as defaults and can be overridden
    /// by individual <see cref="LogEntry"/> instances.
    /// </summary>
    /// <remarks>
    /// A context is typically set once and reused across multiple logs:
    /// 
    /// <code>
    /// logger.SetContext(new LogContext
    /// {
    ///     Source = "AuthService",
    ///     TraceId = "abc-123",
    ///     Metadata = new Dictionary&lt;string, object&gt;
    ///     {
    ///         ["userId"] = "u-001"
    ///     }
    /// });
    /// 
    /// await logger.LogAsync(new LogEntry
    /// {
    ///     Message = "User login started"
    /// });
    /// </code>
    /// 
    /// In this example, the log entry will automatically include the Source, TraceId, and Metadata from the context.
    /// </remarks>
    public record LogContext
    {
        /// <summary>
        /// Logical source of the logs (e.g., service or application name).
        /// </summary>
        public string? Source { get; init; }

        /// <summary>
        /// Correlation identifier for distributed tracing.
        /// </summary>
        public string? TraceId { get; init; }

        /// <summary>
        /// Additional contextual metadata applied to all log entries.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; init; }
    }
}
