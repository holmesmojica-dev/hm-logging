using Hm.Logging.Models;

namespace Hm.Logging.Abstractions
{
    public interface ILoggerService
    {
        Task LogAsync(LogEntry entry, CancellationToken cancellationToken = default);
    }
}
