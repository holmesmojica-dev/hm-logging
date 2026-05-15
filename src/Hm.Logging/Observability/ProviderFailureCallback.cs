using Hm.Logging.Models;

namespace Hm.Logging.Observability;

/// <summary>
/// Represents a callback invoked when a logging provider
/// fails during log processing.
/// </summary>
/// <param name="context">
/// Contextual information describing the provider failure.
/// </param>
/// <param name="cancellationToken">
/// Token used to propagate cancellation notifications.
/// </param>
/// <returns>
/// A task representing the asynchronous callback operation.
/// </returns>
/// <remarks>
/// This callback enables consumers to observe provider failures
/// without interrupting resilient provider execution.
///
/// <para>
/// Exceptions thrown by the callback are not intercepted
/// by the logging pipeline and propagate to the caller.
/// </para>
///
/// <para>
/// The callback is executed individually for each provider failure.
/// </para>
/// </remarks>
public delegate Task ProviderFailureCallback(
    ProviderFailureContext context,
    CancellationToken cancellationToken
);
