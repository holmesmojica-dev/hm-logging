using Hm.Logging.Abstractions;
using Hm.Logging.Configuration;
using Hm.Logging.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Hm.Logging.Extensions
{
    /// <summary>
    /// Provides extension methods to register logging services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the core logging services into the dependency injection container.
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
}
