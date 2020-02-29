using SmartCacheManager.Caching;
using SmartCacheManager.Data;
using SmartCacheManager.Utilities;
using MethodTimer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using SmartCacheManager.Logging;

namespace SmartCacheManager.Services
{
    /// <summary>
    /// Service for CacheSetting
    /// </summary>
    /// <typeparam name="TCacheSetting">Type of CacheSetting</typeparam>
    public class CacheSettingService<TCacheSetting> : ICacheSettingService<TCacheSetting>
        where TCacheSetting : CacheSetting, new()
    {
        protected readonly IGenericStore<TCacheSetting> Store;
        protected readonly ICacheManager CacheManager;
        protected readonly ILogger Logger;
        protected readonly IAsyncLock AsyncLock;
        protected static readonly string CacheKeySupplierType = GenericStore<TCacheSetting>.CacheKeyPrefix + "_BySupplierType-{0}";

        public CacheSettingService(IGenericStore<TCacheSetting> store, ICacheManager cacheManager, ILoggerFactory loggerFactory, IAsyncLock asyncLock)
        {
            Store = store.NotNull(nameof(store));
            CacheManager = cacheManager.NotNull(nameof(cacheManager));
            Logger = loggerFactory.NotNull(nameof(loggerFactory)).CreateLogger(GetType()).NotNull(nameof(Logger));
            AsyncLock = asyncLock.NotNull(nameof(asyncLock));
        }

        /// <summary>
        /// Get CacheSetting from cache by specified SupplierType or if not exists create a global CacheSetting
        /// </summary>
        /// <param name="supplierType">Type of supplier</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>TCacheSetting</returns>
        [Time]
        public async Task<TCacheSetting> GetFromCacheBySupplierTypeAsync<TSupplierType>(TSupplierType supplierType, CancellationToken cancellationToken = default)
        {
            try
            {
                supplierType.NotNull(nameof(supplierType));

                var strSupplierType = supplierType.ConvertTo<string>();

                var cacheKey = string.Format(CacheKeySupplierType, strSupplierType);

                var setting = await CacheManager.GetAsync(cacheKey, async () =>
                {
                    using (await AsyncLock.LockAsync())
                    {
                        var cacheSetting = await Store.TableNoTracking.SingleOrDefaultAsync(p => p.SupplierType == strSupplierType, cancellationToken);

                        if (cacheSetting == null)
                        {
                            cacheSetting = (await Store.TableNoTracking.SingleAsync(p => p.SupplierType == null, cancellationToken)).ShallowCopy<TCacheSetting>();
                            cacheSetting.SupplierType = strSupplierType;
                        }

                        return cacheSetting;
                    }
                }, 1440, cancellationToken).ConfigureAwait(false);

                Logger.SetProperty(LogConstants.CacheSetting, setting, true);
                return setting;
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(GetFromCacheBySupplierTypeAsync)))
            {
                throw;
            }
        }
    }
}
