namespace Hm.Logging.Core;

/// <summary>
/// Represents a disposable logging scope.
/// </summary>
/// <remarks>
/// Restores the previous logging context when disposed,
/// enabling nested scope support within the logging pipeline.
/// </remarks>
internal sealed class LoggingScope(Action onDispose) : IDisposable
{
    private readonly Action _onDispose = onDispose
        ?? throw new ArgumentNullException(nameof(onDispose));

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
            return;

        _onDispose.Invoke();
        _disposed = true;
    }
}
