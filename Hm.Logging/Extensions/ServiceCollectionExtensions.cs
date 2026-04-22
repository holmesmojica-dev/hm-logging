using Hm.Logging.Abstractions;
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
        public static IServiceCollection AddHmLogging(this IServiceCollection services)
        {
            return services
                .AddSingleton<ITraceContext, ActivityTraceContext>();
        }
    }
}
