using SmartCacheManager.Caching;
using SmartCacheManager.Data;
using SmartCacheManager.Utilities;
using MethodTimer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartCacheManager.Logging;

namespace SmartCacheManager.Services
{
    /// <summary>
    /// Service for LimitSetting
    /// </summary>
    /// <typeparam name="TLimitSetting">Type of LimitSetting</typeparam>
    public class LimitSettingService<TLimitSetting> : ILimitSettingService<TLimitSetting>
        where TLimitSetting : LimitSetting, new()
    {
        protected readonly IGenericStore<TLimitSetting> Store;
        protected readonly ICacheManager CacheManager;
        protected readonly ILogger Logger;
        protected readonly IAsyncLock AsyncLock;
        protected static readonly string CacheKeySupplierType = GenericStore<TLimitSetting>.CacheKeyPrefix + "_BySupplierType-{0}";

        public LimitSettingService(IGenericStore<TLimitSetting> store, ICacheManager cacheManager, ILoggerFactory loggerFactory, IAsyncLock asyncLock)
        {
            Store = store.NotNull(nameof(store));
            CacheManager = cacheManager.NotNull(nameof(cacheManager));
            Logger = loggerFactory.NotNull(nameof(loggerFactory)).CreateLogger(GetType()).NotNull(nameof(Logger));
            AsyncLock = asyncLock.NotNull(nameof(asyncLock));
        }

        /// <summary>
        /// Get LimitSetting from cache by specified SupplierType or if not exists create a global LimitSetting
        /// </summary>
        /// <param name="supplierType">Type of supplier</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>List of TLimitSetting</returns>
        [Time]
        public Task<List<TLimitSetting>> GetFromCacheBySupplierTypeAsync<TSupplierType>(TSupplierType supplierType, CancellationToken cancellationToken = default)
        {
            try
            {
                supplierType.NotNull(nameof(supplierType));

                var strSupplierType = supplierType.ConvertTo<string>();

                var cacheKey = string.Format(CacheKeySupplierType, strSupplierType);

                return CacheManager.GetAsync(cacheKey, async () =>
                {
                    using (await AsyncLock.LockAsync())
                    {
                        var list = await Store.TableNoTracking.Where(p => p.Enabled && p.SupplierType == strSupplierType).ToListAsync(cancellationToken);

                        if (list.Count == 0)
                        {
                            var limitSetting = (await Store.TableNoTracking.SingleOrDefaultAsync(p => p.Enabled && p.SupplierType == null, cancellationToken))?.ShallowCopy<TLimitSetting>();

                            if (limitSetting != null)
                            {
                                limitSetting.SupplierType = strSupplierType;
                                list.Add(limitSetting);
                            }
                        }

                        return list;
                    }
                }, 1440, cancellationToken);
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(GetFromCacheBySupplierTypeAsync)))
            {
                throw;
            }
        }
    }
}
