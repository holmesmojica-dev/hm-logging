using System.Collections.Immutable;

namespace Hm.Logging.Models;

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
/// using (logger.BeginScope(new LogContext
/// {
///     Source = "AuthService",
///     TraceId = "trace-123",
///     CorrelationId = "corr-checkout-001",
///     Metadata = ImmutableDictionary&lt;string, object&gt;
///         .Empty
///         .Add("userId", "u-001")
/// }))
/// {
///     await logger.LogAsync(new LogEntry
///     {
///         Message = "User login started"
///     });
/// }
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
    /// Trace identifier associated with the current execution flow.
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Identifier used to correlate related operations
    /// across multiple services, requests, or workflows.
    /// </summary>
    /// <remarks>
    /// CorrelationId values defined within a logging scope
    /// are automatically propagated to all log entries
    /// created within the current execution flow,
    /// unless explicitly overridden by the log entry.
    /// </remarks>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Additional contextual metadata applied to all log entries.
    /// </summary>
    public ImmutableDictionary<string, object>? Metadata { get; init; }
}
