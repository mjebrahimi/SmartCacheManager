using SmartCacheManager.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SmartCacheManager.Services
{
    public static class ServiceConfigurationExtensions
    {
        /// <summary>
        /// Add CacheSettingService and LimitSettingService services
        /// </summary>
        /// <param name="services">services</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddSettingServices(this IServiceCollection services)
        {
            services.TryAddScoped(typeof(ICacheSettingService<>), typeof(CacheSettingService<>));
            services.TryAddScoped(typeof(ILimitSettingService<>), typeof(LimitSettingService<>));
            return services;
        }

        /// <summary>
        /// Add open generics SearchHistoryService
        /// </summary>
        /// <param name="services">services</param>
        /// <param name="useDatabaseSearchHistory">Determine use DatabaseSearchHistoryService[true] or CacheSearchHistoryService[false]</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddSearchHistoryService(this IServiceCollection services, bool useDatabaseSearchHistory = false)
        {
            if (useDatabaseSearchHistory)
                services.TryAddScoped(typeof(ISearchHistoryService<,>), typeof(DatabaseSearchHistoryService<,>));
            else
                services.TryAddScoped(typeof(ISearchHistoryService<,>), typeof(CacheSearchHistoryService<,>));

            return services;
        }

        /// <summary>
        /// Add AsyncLock as scoped to services
        /// </summary>
        /// <param name="services">services</param>
        /// <param name="enableThreadSafety">Determine whether async lock is thread safety</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddAsyncLockScoped(this IServiceCollection services, bool enableThreadSafety = false)
        {
            if (enableThreadSafety)
                services.TryAddScoped<IAsyncLock, SemaphoreSlimAsyncLock>();
            else
                services.TryAddScoped<IAsyncLock, NullAsyncLock>();

            return services;
        }
    }
}
