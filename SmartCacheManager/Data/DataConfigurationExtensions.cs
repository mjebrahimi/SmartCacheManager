using SmartCacheManager.Caching;
using SmartCacheManager.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using NeuroSpeech.EFCoreLiveMigration;
using System.Linq;

namespace SmartCacheManager.Data
{
    public static class DataConfigurationExtensions
    {
        /// <summary>
        /// Add SmartCacheManager DbContext and Stores to services
        /// </summary>
        /// <param name="services">services</param>
        /// <param name="optionsAction">Action to configure SmartCacheManagerDbContext</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddSmartCacheManagerDbContext(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
        {
            return services.AddSmartCacheManagerDbContext<SmartCacheManagerDbContext>(optionsAction);
        }

        /// <summary>
        /// Add SmartCacheManager DbContext and Stores to services
        /// </summary>
        /// <typeparam name="TDbContext">Type of cache manager dbContext</typeparam>
        /// <param name="services">services</param>
        /// <param name="optionsAction">Action to configure SmartCacheManagerDbContext</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddSmartCacheManagerDbContext<TDbContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
            where TDbContext : DbContext
        {
            services.NotNull(nameof(services));
            optionsAction.NotNull(nameof(optionsAction));

            services.AddDbContext<TDbContext>(optionsAction);

            return services.AddSmartCacheManagerStores<TDbContext>();
        }

        /// <summary>
        /// Add stores for specified TDbContext to services
        /// </summary>
        /// <typeparam name="TDbContext">TDbContext</typeparam>
        /// <param name="services">services</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddSmartCacheManagerStores<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
        {
            services.NotNull(nameof(services));

            services.AddSmartCacheManagerStore<TDbContext, CacheSetting>();
            services.AddSmartCacheManagerStore<TDbContext, LimitSetting>();
            services.AddSmartCacheManagerStore<TDbContext, IncomingRequest>();
            services.AddSmartCacheManagerStore<TDbContext, OutgoingRequest>();

            return services;
        }

        /// <summary>
        /// Add a store for specified TEntity and TDbContext to services
        /// </summary>
        /// <typeparam name="TDbContext">TDbContext</typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="services">services</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddSmartCacheManagerStore<TDbContext, TEntity>(this IServiceCollection services)
            where TDbContext : DbContext
            where TEntity : class, new()
        {
            services.NotNull(nameof(services));

            services.TryAddScoped<IGenericStore<TEntity>>(serviceProvider =>
            {
                var dbContext = serviceProvider.GetRequiredService<TDbContext>();
                var cacheManager = serviceProvider.GetRequiredService<ICacheManager>();

                return new GenericStore<TEntity>(dbContext, cacheManager);
            });

            return services;
        }

        /// <summary>
        /// Migrate database to latest and seed data
        /// </summary>
        /// <typeparam name="TDbContext">Type of cache manager dbContext</typeparam>
        /// <param name="serviceProvider">serviceProvider</param>
        public static void MigrateAndSeedData<TDbContext>(this IServiceProvider serviceProvider)
            where TDbContext : DbContext
        {
            serviceProvider.NotNull(nameof(serviceProvider));
            using (var scope = serviceProvider.CreateScope())
            {
                using (var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>())
                {
                    //Auto migrate dbContext to the latest version of model changes on the fly
                    if (dbContext.Database.IsSqlServer())
                        MigrationHelper.ForSqlServer(dbContext).Migrate();

                    //Seed initial data
                    var cacheSettingStore = scope.ServiceProvider.GetRequiredService<IGenericStore<CacheSetting>>();
                    if (!cacheSettingStore.Table.Any())
                    {
                        cacheSettingStore.Add(new CacheSetting
                        {
                            MinSearchDiffHours = 1 * 24, //24 (1 day)
                            MaxSearchDiffHours = 30 * 24, //480 (30 day)
                            MinCacheMinutes = 5,
                            MaxCacheMinutes = 24 * 60, //1440 (1 day)
                            OverSearchDiffHours = 90 * 24, //2160 (90 day)
                            RecentSearchMinimumRPM = 1,
                            RecentSearchMaxmimumRPM = 20,
                            RpmDurationMinutes = 2,
                            SupplierType = null,
                        });
                    }
                }
            }
        }
    }
}
