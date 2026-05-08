namespace Hm.Logging.Core
{
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

            _onDispose.Invoke();
            _disposed = true;
        }
    }
}
