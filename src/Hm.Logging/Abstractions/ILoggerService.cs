using Hm.Logging.Models;
using Hm.Logging.Observability;

namespace Hm.Logging.Abstractions;

/// <summary>
/// Defines the contract for structured logging operations.
/// </summary>
/// <remarks>
/// Implementations are responsible for:
/// <list type="bullet">
/// <item>
/// <description>Applying contextual information from active scopes.</description>
/// </item>
/// <item>
/// <description>Validating and normalizing log entries.</description>
/// </item>
/// <item>
/// <description>Dispatching log entries through the configured logging providers.</description>
/// </item>
/// </list>
///
/// <para>
/// Logging scopes allow shared contextual information such as
/// <see cref="LogContext.Source"/>,
/// <see cref="LogContext.TraceId"/>,
/// <see cref="LogContext.CorrelationId"/>,
/// and shared metadata to be automatically applied
/// to all logs created within the current execution flow.
/// </para>
///
/// <para>
/// When values exist in both <see cref="LogContext"/> and <see cref="LogEntry"/>,
/// the values defined in <see cref="LogEntry"/> take precedence,
/// including TraceId, CorrelationId, Source, and Metadata values.
/// </para>
/// </remarks>
public interface ILoggerService
{
    /// <summary>
    /// Begins a logging scope using the specified contextual information.
    /// </summary>
    /// <param name="context">
    /// The contextual information to apply to all logs created
    /// within the current execution flow.
    /// </param>
    /// <returns>
    /// An <see cref="IDisposable"/> that ends the scope when disposed.
    /// </returns>
    /// <remarks>
    /// Scopes are intended to avoid repeating shared information
    /// such as trace identifiers, sources, or common metadata.
    ///
    /// <para>
    /// Example:
    /// </para>
    ///
    /// <code>
    /// using (logger.BeginScope(new LogContext
    /// {
    ///     Source = "AuthService",
    ///     TraceId = "trace-123",
    ///     CorrelationId = "corr-checkout-001"
    /// }))
    /// {
    ///     await logger.LogAsync(LogEntry.Info("User login started"));
    /// }
    /// </code>
    ///
    /// <para>
    /// The scope applies only to the current execution flow
    /// and is automatically removed when disposed.
    /// </para>
    ///
    /// <para>
    /// Nested scopes are supported.
    ///
    /// When multiple scopes are created within the same execution flow,
    /// inner scopes temporarily override contextual values while preserving
    /// the parent scope state.
    ///
    /// Once the inner scope is disposed, the previous scope context is restored.
    /// </para>
    ///
    /// <code>
    /// using (logger.BeginScope(new LogContext
    /// {
    ///     Source = "OrderService"
    /// }))
    /// {
    ///     using (logger.BeginScope(new LogContext
    ///     {
    ///         TraceId = "child-trace"
    ///     }))
    ///     {
    ///         await logger.LogAsync(LogEntry.Info("Processing child operation"));
    ///     }
    ///
    ///     // The original Source context is restored here.
    ///     await logger.LogAsync(LogEntry.Info("Processing parent operation"));
    /// }
    /// </code>
    /// </remarks>
    IDisposable BeginScope(LogContext context);


    /// <summary>
    /// Logs an entry asynchronously through the configured logging pipeline.
    /// </summary>
    /// <param name="entry">
    /// Log entry to be processed.
    /// </param>
    /// <param name="providerFailureCallback">
    /// Optional callback invoked when a logging provider fails during execution.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to propagate cancellation notifications.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous logging operation.
    /// </returns>
    /// <remarks>
    /// Provider execution failures are handled internally
    /// to preserve resilient logging behavior.
    ///
    /// <para>
    /// Remaining providers continue executing independently
    /// even when individual providers fail.
    /// </para>
    ///
    /// <para>
    /// When a provider failure callback is supplied,
    /// provider failure diagnostics are exposed to the consumer.
    /// </para>
    ///
    /// <para>
    /// The callback is invoked individually for each provider failure.
    /// </para>
    ///
    /// <para>
    /// Exceptions thrown by the callback are not intercepted
    /// by the logging pipeline and propagate to the caller.
    /// </para>
    /// </remarks>
    Task LogAsync(
        LogEntry entry,
        ProviderFailureCallback? providerFailureCallback = null,
        CancellationToken cancellationToken = default);
}