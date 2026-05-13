using Hm.Logging.Models;

namespace Hm.Logging.Abstractions
{
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
        /// Token to cancel the operation.
        /// </param>
        Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default);
    }
}
