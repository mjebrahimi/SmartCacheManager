using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace SmartCacheManager.Utilities
{
    /// <summary>
    /// Abstracts the system clock to facilitate testing.
    /// </summary>
    public interface ISystemClock
    {
        DateTime DateTimeNow { get; }
        DateTimeOffset DateTimeOffsetNow { get; }
    }

    /// <summary>
    /// Default implementation for ISystemClock (return DateTime.Now)
    /// </summary>
    public class SystemClock : ISystemClock
    {
        public DateTime DateTimeNow => DateTime.Now; //or DateTime.UtcNow;
        public DateTimeOffset DateTimeOffsetNow => DateTimeOffset.Now;
    }

    public static class SystemClockConfigurationExtension
    {
        /// <summary>
        /// Add DefaultSystemClock as ISystemClock to specified IServiceCollection
        /// </summary>
        /// <param name="services">services</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddSystemClock(this IServiceCollection services)
        {
            services.TryAddSingleton<ISystemClock, SystemClock>();
            return services;
        }
    }
}
