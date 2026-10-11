using Hm.Logging.Abstractions;
using Hm.Logging.Models;

namespace Hm.Logging.Tests.Mocks;

public sealed class CancellationLogProvider(
    CancellationTokenSource cancellationTokenSource,
    Guid? providerId = null) : ILogProvider
{
    public Guid ProviderId { get; } = providerId ?? Guid.NewGuid();

    public Task WriteAsync(
        LogEntry entry,
        CancellationToken cancellationToken)
    {
        cancellationTokenSource.Cancel();
        throw new OperationCanceledException(cancellationToken);
    }
}
