namespace Hm.Logging.Models;

/// <summary>
/// Represents contextual information about a logging provider failure.
/// </summary>
/// <remarks>
/// This model is used to expose provider execution failures through
/// resilient logging diagnostics without interrupting the main
/// application execution flow.
/// </remarks>
public sealed record ProviderFailureContext
{
    /// <summary>
    /// Gets the identifier of the registered provider instance that generated the failure.
    /// </summary>
    /// <remarks>
    /// This value matches <see cref="Abstractions.ILogProvider.ProviderId"/> and can distinguish multiple registered
    /// instances that use the same implementation type.
    /// </remarks>
    public required Guid ProviderId { get; init; }

    /// <summary>
    /// Gets the exception thrown by the provider.
    /// </summary>
    public required Exception Exception { get; init; }

    /// <summary>
    /// Gets the log entry associated with the provider failure.
    /// </summary>
    public required LogEntry LogEntry { get; init; }
}
