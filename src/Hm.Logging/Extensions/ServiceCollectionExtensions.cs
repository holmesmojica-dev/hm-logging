using Hm.Logging.Abstractions;
using Hm.Logging.Configuration;
using Hm.Logging.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Hm.Logging.Extensions;

/// <summary>
/// Provides extension methods to register logging services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core logging pipeline and infrastructure services into the dependency injection container.
    /// </summary>
    /// <param name="services">
    /// The dependency injection service collection.
    /// </param>
    /// <param name="configure">
    /// Optional configuration delegate used to customize
    /// <see cref="LoggingOptions"/>.
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> instance.
    /// </returns>
    /// <remarks>
    /// This method registers HM Logging Core services and configuration, but it does not register a logging
    /// destination. At least one <see cref="ILogProvider"/> implementation must be registered independently through
    /// dependency injection before <see cref="ILoggerService"/> is resolved. Resolving the service without a
    /// registered provider is an invalid configuration and causes service construction to throw
    /// <see cref="InvalidOperationException"/>.
    ///
    /// <para>
    /// Multiple providers can coexist simultaneously,
    /// allowing the logging pipeline to distribute log entries
    /// across different logging targets.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddHmLogging(this IServiceCollection services, Action<LoggingOptions>? configure = null)
    {
        LoggingOptions options = new();
        configure?.Invoke(options);

        return services
            .AddSingleton(options)
            .AddSingleton<ITraceContext, ActivityTraceContext>()
            .AddScoped<ILoggerService, LoggerService>();
    }
}
