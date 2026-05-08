using System.Collections.Immutable;
using Hm.Logging.Abstractions;
using Hm.Logging.Configuration;
using Hm.Logging.Extensions;
using Hm.Logging.Models;

namespace Hm.Logging.Core
{
    internal sealed class LoggerService(ILogRepository logRepository, ITraceContext traceContext, LoggingOptions options) : ILoggerService
    {
        private readonly ILogRepository _logRepository = logRepository
            ?? throw new ArgumentNullException(nameof(logRepository));

        private readonly ITraceContext _traceContext = traceContext
            ?? throw new ArgumentNullException(nameof(traceContext));

        private readonly LoggingOptions _options = options
            ?? throw new ArgumentNullException(nameof(options));

        private readonly AsyncLocal<LogContext?> _currentContext = new();


        public IDisposable BeginScope(LogContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _currentContext.Value = context;
            return new LoggingScope(() => _currentContext.Value = null);
        }


        public async Task LogAsync(LogEntry entry, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entry);

            if (entry.Level < _options.MinimumLevel)
            {
                return;
            }

            LogEntry mergedEntry = MergeContext(entry, _currentContext.Value);
            LogEntry normalizedEntry = mergedEntry.EnsureValid(_traceContext);

            await _logRepository.SaveAsync(normalizedEntry, cancellationToken).ConfigureAwait(false);
        }


        private static LogEntry MergeContext(LogEntry entry, LogContext? context)
        {
            return context is null ? entry : (entry with
            {
                TraceId = entry.TraceId ?? context.TraceId,
                Source = entry.Source ?? context.Source,
                Metadata = MergeMetadata(context.Metadata, entry.Metadata)
            });
        }


        private static ImmutableDictionary<string, object>? MergeMetadata(
           ImmutableDictionary<string, object>? contextMetadata,
           ImmutableDictionary<string, object>? entryMetadata)
        {
            if (contextMetadata is null && entryMetadata is null)
            {
                return null;
            }

            ImmutableDictionary<string, object>.Builder builder = ImmutableDictionary.CreateBuilder<string, object>();

            if (contextMetadata is not null)
            {
                foreach (KeyValuePair<string, object> item in contextMetadata)
                {
                    builder[item.Key] = item.Value;
                }
            }

            if (entryMetadata is not null)
            {
                foreach (KeyValuePair<string, object> item in entryMetadata)
                {
                    builder[item.Key] = item.Value;
                }
            }

            return builder.Count > 0
                ? builder.ToImmutable()
                : null;
        }
    }

}
