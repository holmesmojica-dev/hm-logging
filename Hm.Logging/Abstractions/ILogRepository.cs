using Hm.Logging.Models;

namespace Hm.Logging.Abstractions
{
    /// <summary>
    /// Defines a contract for persisting log entries to a storage medium.
    /// </summary>
    internal interface ILogRepository
    {
        /// <summary>
        /// Persists a log entry asynchronously.
        /// </summary>
        /// <param name="entry">
        /// The <see cref="LogEntry"/> to be stored.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to cancel the operation.
        /// </param>
        Task SaveAsync(LogEntry entry, CancellationToken cancellationToken = default);
    }
}
