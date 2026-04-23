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
        /// Sets the logging context to be applied to all subsequent log entries
        /// within the current execution flow.
        /// </summary>
        /// <param name="context">
        /// The <see cref="LogContext"/> containing shared values such as Source,
        /// TraceId, and Metadata.
        /// </param>
        /// <remarks>
        /// The context provides default values that will be automatically applied
        /// to each <see cref="LogEntry"/>.
        ///
        /// <para>
        /// When a property exists in both <see cref="LogContext"/> and <see cref="LogEntry"/>,
        /// the value defined in <see cref="LogEntry"/> takes precedence.
        /// </para>
        ///
        /// <para>
        /// For example:
        /// </para>
        /// <code>
        /// logger.SetContext(new LogContext
        /// {
        ///     Source = "AuthService",
        ///     TraceId = "abc-123"
        /// });
        ///
        /// await logger.LogAsync(new LogEntry
        /// {
        ///     Message = "Special operation",
        ///     TraceId = "override-999"
        /// });
        /// </code>
        ///
        /// <para>
        /// Resulting log:
        /// </para>
        /// <code>
        /// {
        ///   "message": "Special operation",
        ///   "source": "AuthService",
        ///   "traceId": "override-999"
        /// }
        /// </code>
        ///
        /// <para>
        /// Metadata from both context and entry will be merged. If the same key exists,
        /// the value from <see cref="LogEntry"/> overrides the one from <see cref="LogContext"/>.
        /// </para>
        ///
        /// <para>
        /// This method is typically called once per execution scope
        /// (e.g., per HTTP request, background job, or transaction).
        /// </para>
        ///
        /// <para>
        /// Internally, the context is stored per execution flow, ensuring that
        /// concurrent operations do not interfere with each other.
        /// </para>
        /// </remarks>
        void SetContext(LogContext context);

        /// <summary>
        /// Logs a structured log entry asynchronously.
        /// </summary>
        /// <param name="entry">The log entry containing all relevant data.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task LogAsync(LogEntry entry, CancellationToken cancellationToken = default);
    }
}
