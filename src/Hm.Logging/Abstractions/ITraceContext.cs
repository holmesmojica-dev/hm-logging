namespace Hm.Logging.Abstractions;

/// <summary>
/// Provides access to the current distributed tracing context
/// used for log correlation and observability.
/// </summary>
/// <remarks>
/// This abstraction allows the logging pipeline to retrieve
/// trace identifiers independently of any specific tracing implementation.
///
/// <para>
/// Implementations may integrate with distributed tracing systems
/// such as <see cref="System.Diagnostics.Activity"/>,
/// OpenTelemetry, custom correlation pipelines,
/// or external observability platforms.
/// </para>
///
/// <para>
/// When no trace identifier is available,
/// the logging pipeline may generate a fallback trace identifier
/// during log normalization.
/// </para>
/// </remarks>
public interface ITraceContext
{
    /// <summary>
    /// Gets the current distributed trace identifier.
    /// </summary>
    /// <returns>
    /// The current trace identifier if available;
    /// otherwise <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// Trace identifiers are commonly used to correlate
    /// logs, telemetry, and distributed operations
    /// across execution boundaries and services.
    /// </remarks>
    string? GetTraceId();
}