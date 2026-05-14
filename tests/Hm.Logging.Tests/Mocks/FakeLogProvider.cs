using Hm.Logging.Abstractions;
using Hm.Logging.Models;

namespace Hm.Logging.Tests.Mocks;

internal sealed class FakeLogProvider : ILogProvider
{
    public List<LogEntry> Entries { get; } = [];

    public bool ThrowException { get; set; }

    public Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default)
    {
        if (ThrowException)
        {
            throw new InvalidOperationException("Provider failure.");
        }

        Entries.Add(entry);

        return Task.CompletedTask;
    }
}
