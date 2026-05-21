namespace Hm.Logging.Core.Scopes;

/// <summary>
/// Represents an active logging scope lifetime.
/// </summary>
/// <remarks>
/// When disposed, restores the previous scope within the
/// internal nested scope propagation chain.
///
/// <para>
/// This enables predictable parent scope restoration
/// across asynchronous execution flows.
/// </para>
/// </remarks>
internal sealed class LoggingScope(Action onDispose) : IDisposable
{
    private readonly Action _onDispose = onDispose
        ?? throw new ArgumentNullException(nameof(onDispose));

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            _onDispose.Invoke();
        }
        finally
        {
            _disposed = true;
        }
    }
}
