using SmartCacheManager.Caching.EasyCaching;
using SmartCacheManager.Data;
using SmartCacheManager.Services;
using SmartCacheManager.Utilities;
using EasyCaching.Core.Serialization;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NeuroSpeech.EFCoreLiveMigration;
using System;
using System.Linq;
using SmartCacheManager.Logging.Serilog;

namespace SmartCacheManager
{
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Add all required services for SmartCacheManager like Data/Stores and EasyCaching and Serilog, ...
        /// </summary>
        /// <param name="services">services</param>
        /// <param name="dbContextConfigure">Action to configure CacheManagerDbContext</param>
        /// <param name="cachingConfigure">Action to configure EasyCachingOptions</param>
        /// <param name="loggerConfigure">Action to configure SerilogOptions</param>
        /// <param name="enableThreadSafety">Determine whether async lock is thread safety</param>
        /// <param name="useDatabaseSearchHistory">Determine use DatabaseSearchHistoryService[false] or CacheSearchHistoryService[true]</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddSmartCacheManager(this IServiceCollection services, Action<DbContextOptionsBuilder> dbContextConfigure,
            Action<EasyCachingOptions> cachingConfigure = null, Action<SerilogOptions> loggerConfigure = null, bool enableThreadSafety = false, bool useDatabaseSearchHistory = false)
        {
            return services.AddSmartCacheManager<SmartCacheManagerDbContext>(dbContextConfigure, cachingConfigure, loggerConfigure, enableThreadSafety, useDatabaseSearchHistory);
        }

        /// <summary>
        /// Add all required services for SmartCacheManager like Data/Stores and EasyCaching and Serilog, ...
        /// </summary>
        /// <typeparam name="TDbContext">Type of cache manager dbContext</typeparam>
        /// <param name="services">services</param>
        /// <param name="dbContextConfigure">Action to configure CacheManagerDbContext</param>
        /// <param name="cachingConfigure">Action to configure EasyCachingOptions</param>
        /// <param name="loggerConfigure">Action to configure SerilogOptions</param>
        /// <param name="enableThreadSafety">Determine whether async lock is thread safety</param>
        /// <param name="useDatabaseSearchHistory">Determine use DatabaseSearchHistoryService[false] or CacheSearchHistoryService[true]</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddSmartCacheManager<TDbContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> dbContextConfigure,
            Action<EasyCachingOptions> cachingConfigure = null, Action<SerilogOptions> loggerConfigure = null, bool enableThreadSafety = false, bool useDatabaseSearchHistory = false)
            where TDbContext : DbContext
        {
            services.NotNull(nameof(services));
            services.NotNull(nameof(dbContextConfigure));

            services.AddSystemClock();
            services.AddSmartCacheManagerDbContext<TDbContext>(dbContextConfigure);

            services.AddSettingServices();
            services.AddSearchHistoryService(useDatabaseSearchHistory);
            services.AddAsyncLockScoped(enableThreadSafety);

            services.AddEasyCachingCacheManager(cachingConfigure);

            var serilogConfigure = SetDefaultSqlServerConnectionForSerilog(dbContextConfigure, loggerConfigure);
            services.AddSerilogLogger(serilogConfigure);

            return services;
        }

        /// <summary>
        /// Set default sql connection string of serilog configure from dbContext configure if SqlServer used;
        /// </summary>
        /// <param name="dbContextConfigure">Action to configure CacheManagerDbContext</param>
        /// <param name="loggerConfigure">Action to configure SerilogOptions</param>
        /// <returns></returns>
        private static Action<SerilogOptions> SetDefaultSqlServerConnectionForSerilog(Action<DbContextOptionsBuilder> dbContextConfigure, Action<SerilogOptions> loggerConfigure = null)
        {
            return opt =>
            {
                var optionsBuilder = new DbContextOptionsBuilder();
                dbContextConfigure(optionsBuilder);

                var sqlServerOptionsExtension = optionsBuilder.Options.FindExtension<SqlServerOptionsExtension>();
                if (sqlServerOptionsExtension != null && opt.SqlConnectionString == null)
                    opt.SqlConnectionString = sqlServerOptionsExtension.ConnectionString;

                loggerConfigure?.Invoke(opt);
            };
        }

        /// <summary>
        /// Initialize database and seed initial data
        /// </summary>
        /// <param name="app">ApplicationBuilder</param>
        public static void InitializeSmartCacheManager(this IApplicationBuilder app)
        {
            app.InitializeSmartCacheManager<SmartCacheManagerDbContext>();
        }

        /// <summary>
        /// Initialize database and seed initial data
        /// </summary>
        /// <typeparam name="TDbContext">Type of cache manager dbContext</typeparam>
        /// <param name="app">ApplicationBuilder</param>
        public static void InitializeSmartCacheManager<TDbContext>(this IApplicationBuilder app)
            where TDbContext : DbContext
        {
            //WORKAROUND: fixing the time zone lose problem of DateTime in version of <= v0.8.0 of EasyCaching.Serialization.MessagePack
            app.ApplicationServices.Fix_EasyCaching_MessagePack_TimeZone_LoseProblem();

            app.ApplicationServices.MigrateAndSeedData<TDbContext>();
        }
    }
}
