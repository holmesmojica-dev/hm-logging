using Hm.Logging.Abstractions;
using Hm.Logging.Models;

namespace Hm.Logging.Tests.Mocks;

public sealed class CancellationLogProvider(
    CancellationTokenSource cancellationTokenSource) : ILogProvider
{
    public Task WriteAsync(
        LogEntry entry,
        CancellationToken cancellationToken)
    {
        cancellationTokenSource.Cancel();
        throw new OperationCanceledException(cancellationToken);
    }
}
