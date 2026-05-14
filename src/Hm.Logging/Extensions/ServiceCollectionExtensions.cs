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
    /// Custom logging providers can be registered independently
    /// through dependency injection using <see cref="ILogProvider"/>.
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
            .AddScoped<ITraceContext, ActivityTraceContext>()
            .AddScoped<ILoggerService, LoggerService>();
    }
}
