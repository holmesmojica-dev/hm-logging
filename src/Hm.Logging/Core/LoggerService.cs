using System.Collections.Immutable;
using Hm.Logging.Abstractions;
using Hm.Logging.Configuration;
using Hm.Logging.Extensions;
using Hm.Logging.Models;
using Hm.Logging.Observability;

namespace Hm.Logging.Core;

/// <summary>
/// Default implementation of <see cref="ILoggerService"/> responsible for
/// orchestrating log validation, normalization, scope propagation,
/// metadata enrichment, and provider dispatch execution.
/// </summary>
/// <remarks>
/// Provider execution failures are handled internally to preserve
/// resilient logging behavior and prevent logging infrastructure
/// failures from interrupting application execution flow.
///
/// <para>
/// Optional provider failure diagnostics can be observed through
/// the provider failure callback available in <see cref="LogAsync"/>.
/// </para>
///
/// <para>
/// Provider failures are isolated per provider execution,
/// allowing remaining providers to continue processing logs
/// even when individual providers fail.
/// </para>
/// </remarks>
internal sealed class LoggerService(
    IEnumerable<ILogProvider> logProviders,
    ITraceContext traceContext,
    LoggingOptions options) : ILoggerService
{
    private readonly IReadOnlyCollection<ILogProvider> _logProviders = logProviders?.ToArray()
        ?? throw new ArgumentNullException(nameof(logProviders));

    private readonly ITraceContext _traceContext = traceContext
        ?? throw new ArgumentNullException(nameof(traceContext));

    private readonly LoggingOptions _options = options
        ?? throw new ArgumentNullException(nameof(options));

    private readonly AsyncLocal<LogContext?> _currentContext = new();


    public IDisposable BeginScope(LogContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        LogContext? previousContext = _currentContext.Value;
        _currentContext.Value = context;

        return new LoggingScope(() => _currentContext.Value = previousContext);
    }


    public async Task LogAsync(
        LogEntry entry,
        ProviderFailureCallback? providerFailureCallback = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        cancellationToken.ThrowIfCancellationRequested();

        if (entry.Level < _options.MinimumLevel)
        {
            return;
        }

        LogEntry mergedEntry = MergeContext(entry, _currentContext.Value);
        LogEntry normalizedEntry = mergedEntry.EnsureValid(_traceContext);

        ValidateMessageLength(normalizedEntry.Message);

        foreach (ILogProvider logProvider in _logProviders)
        {
            try
            {
                await logProvider.WriteAsync(normalizedEntry, cancellationToken);
            }
            catch (Exception exception)
            {
                if (providerFailureCallback is null)
                {
                    continue;
                }

                await providerFailureCallback(
                    new ProviderFailureContext
                    {
                        ProviderType = logProvider.GetType(),
                        Exception = exception,
                        LogEntry = normalizedEntry
                    },
                    cancellationToken);
            }
        }
    }


    private void ValidateMessageLength(string message)
    {
        if (_options.MaxMessageLength == 0)
            return;

        if (message.Length > _options.MaxMessageLength)
        {
            throw new ArgumentException(
                $"Log message exceeds the maximum allowed length of {_options.MaxMessageLength} characters.",
                nameof(message));
        }
    }


    private static LogEntry MergeContext(LogEntry entry, LogContext? context)
    {
        return context is null ? entry : (entry with
        {
            TraceId = entry.TraceId ?? context.TraceId,
            CorrelationId = entry.CorrelationId ?? context.CorrelationId,
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
            AddMetadataEntries(builder, contextMetadata);
        }

        if (entryMetadata is not null)
        {
            AddMetadataEntries(builder, entryMetadata);
        }

        return builder.Count > 0
            ? builder.ToImmutable()
            : null;
    }


    private static void AddMetadataEntries(
        ImmutableDictionary<string, object>.Builder builder,
        ImmutableDictionary<string, object> metadata)
    {
        foreach (KeyValuePair<string, object> item in metadata)
        {
            builder[item.Key] = item.Value;
        }
    }
}
