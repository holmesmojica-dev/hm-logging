using Hm.Logging.Models;

namespace Hm.Logging.Abstractions;

/// <summary>
/// Defines a contract for processing log entries through a logging provider.
/// </summary>
/// <remarks>
/// Multiple logging providers can be registered simultaneously.
///
/// <para>
/// The logging pipeline distributes each log entry to all registered providers,
/// allowing independent processing across different logging targets such as
/// files, databases, consoles, gRPC transports, or observability systems.
/// </para>
///
/// <para>
/// Provider failures are isolated internally by the logging pipeline
/// to prevent individual provider failures from interrupting
/// execution of remaining providers.
/// </para>
///
/// <para>
/// Providers should remain lightweight, isolated, and provider-specific,
/// while the logging pipeline preserves validation, normalization,
/// contextual enrichment, and orchestration responsibilities.
/// </para>
/// </remarks>
public interface ILogProvider
{
    /// <summary>
    /// Processes a log entry asynchronously.
    /// </summary>
    /// <param name="entry">
    /// The <see cref="LogEntry"/> to process.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to propagate cancellation notifications
    /// during provider execution.
    /// </param>
    /// <remarks>
    /// Implementations should honor cancellation requests whenever possible
    /// to avoid blocking the logging pipeline during shutdown,
    /// request cancellation, or background worker termination.
    /// </remarks>
    Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default);
}
