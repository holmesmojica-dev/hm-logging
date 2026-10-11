using Hm.Logging.Models;

namespace Hm.Logging.Abstractions;

/// <summary>
/// Defines a contract for processing log entries through a logging provider.
/// </summary>
/// <remarks>
/// Multiple logging providers can be registered simultaneously.
///
/// <para>
/// By default, the logging pipeline distributes each log entry to all registered providers. Consumers may also
/// select registered instances by <see cref="ProviderId"/>, allowing independent processing across different
/// logging targets such as files, databases, consoles, gRPC transports, or observability systems.
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
    /// Gets the stable identifier of this registered provider instance.
    /// </summary>
    /// <remarks>
    /// The identifier distinguishes provider instances rather than implementation types, so multiple instances of
    /// the same provider can be selected independently. It must be nonempty and unique among all providers
    /// registered in the same HM Logging pipeline.
    /// </remarks>
    Guid ProviderId { get; }

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
