using Hm.Logging.Models;

namespace Hm.Logging.Abstractions
{
    /// <summary>
    /// Defines the contract for logging structured events.
    /// Implementations may store logs in files, databases, or external systems.
    /// </summary>
    public interface ILoggerService
    {
        /// <summary>
        /// Logs a structured log entry asynchronously.
        /// </summary>
        /// <param name="entry">The log entry containing all relevant data.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task LogAsync(LogEntry entry, CancellationToken cancellationToken = default);
    }
}
