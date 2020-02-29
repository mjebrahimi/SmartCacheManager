using SmartCacheManager.Caching;
using SmartCacheManager.Data;
using SmartCacheManager.Utilities;
using MethodTimer;
using System;
using System.Threading;
using System.Threading.Tasks;
using SmartCacheManager.Logging;

namespace SmartCacheManager.Services
{
    /// <summary>
    /// Cache implementation of ISearchHistoryService
    /// </summary>
    /// <typeparam name="TCacheSetting">Type of CacheSetting</typeparam>
    /// <typeparam name="TLimitSetting">Type of LimitSetting</typeparam>
    public class CacheSearchHistoryService<TCacheSetting, TLimitSetting> : ISearchHistoryService<TCacheSetting, TLimitSetting>
        where TCacheSetting : CacheSetting, new()
        where TLimitSetting : LimitSetting, new()
    {
        protected readonly ICacheManager CacheManager;
        protected readonly ILogger Logger;
        protected readonly ICacheSettingService<TCacheSetting> CacheSettingService;
        protected readonly ILimitSettingService<TLimitSetting> LimitSettingService;

        public CacheSearchHistoryService(ICacheManager cacheManager, ILoggerFactory loggerFactory,
            ICacheSettingService<TCacheSetting> cacheSettingService, ILimitSettingService<TLimitSetting> limitSettingService)
        {
            CacheManager = cacheManager.NotNull(nameof(cacheManager));
            Logger = loggerFactory.NotNull(nameof(loggerFactory)).CreateLogger(GetType()).NotNull(nameof(Logger));
            CacheSettingService = cacheSettingService.NotNull(nameof(cacheSettingService));
            LimitSettingService = limitSettingService.NotNull(nameof(limitSettingService));
        }

        /// <summary>
        /// Add an incoming search request
        /// </summary>
        /// <typeparam name="TSupplierType">Type of SupplierType</typeparam>
        /// <typeparam name="TSearchModel">Type of SearchModel</typeparam>
        /// <param name="incomingPrefix">Icoming pattern prefix</param>
        /// <param name="searchModel">Search model</param>
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        [Time]
        public async Task AddIncommingAsync<TSupplierType, TSearchModel>(string incomingPrefix, TSearchModel searchModel, TSupplierType supplierType, CancellationToken cancellationToken = default)
        {
            try
            {
                incomingPrefix.NotNullOrWhiteSpace(nameof(incomingPrefix));
                supplierType.NotNull(nameof(supplierType));
                searchModel.NotNull(nameof(searchModel));

                var strSupplierType = supplierType.ConvertTo<string>();

                Logger.SetProperty(LogConstants.SearchModel, searchModel, true);
                Logger.SetProperty(LogConstants.SupplierType, strSupplierType);

                var cacheSetting = await CacheSettingService.GetFromCacheBySupplierTypeAsync(strSupplierType, cancellationToken).ConfigureAwait(false);

                var key = incomingPrefix + "-" + Guid.NewGuid().ToString("N");
                await CacheManager.SetAsync(key, string.Empty, cacheSetting.RpmDurationMinutes, cancellationToken).ConfigureAwait(false);

                ////3 times to try
                //if (await TrySet().ConfigureAwait(false) == false)
                //    if (await TrySet().ConfigureAwait(false) == false)
                //        if (await TrySet().ConfigureAwait(false) == false)
                //            throw new Exception($"Faild to set cache {nameof(AddSearchHistoryAsync)}");
                //Task<bool> TrySet() => CacheManager.TrySetAsync(key, string.Empty, cacheSetting.RpmDurationMinutes, cancellationToken);
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(AddIncommingAsync)))
            {
                throw;
            }
        }

        /// <summary>
        /// Add an outgoing search request
        /// </summary>
        /// <typeparam name="TSupplierType">Type of SupplierType</typeparam>
        /// <typeparam name="TSearchModel">Type of SearchModel</typeparam>
        /// <param name="outgoingPrefix">Outgoing pattern prefix</param>
        /// <param name="searchModel">Search model</param>
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        [Time]
        public async Task AddOutgoingAsync<TSupplierType, TSearchModel>(string outgoingPrefix, TSearchModel searchModel, TSupplierType supplierType, CancellationToken cancellationToken = default)
        {
            try
            {
                outgoingPrefix.NotNullOrWhiteSpace(nameof(outgoingPrefix));
                supplierType.NotNull(nameof(supplierType));
                searchModel.NotNull(nameof(searchModel));

                var strSupplierType = supplierType.ConvertTo<string>();

                Logger.SetProperty(LogConstants.SearchModel, searchModel, true);
                Logger.SetProperty(LogConstants.SupplierType, strSupplierType);

                var limitSettings = await LimitSettingService.GetFromCacheBySupplierTypeAsync(strSupplierType, cancellationToken).ConfigureAwait(false); //Can be null

                foreach (var limitSetting in limitSettings)
                {
                    var key = outgoingPrefix + $"{limitSetting.Id}_{Guid.NewGuid().ToString("N")}";
                    var expirationMinutes = Convert.ToInt32(limitSetting.LimitDuration.TotalMinutes);
                    await CacheManager.SetAsync(key, string.Empty, expirationMinutes, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(AddOutgoingAsync)))
            {
                throw;
            }
        }

        /// <summary>
        /// Get current RPM based on specified prefix and supplier type
        /// </summary>
        /// <typeparam name="TSupplierType">Type of SupplierType</typeparam>
        /// <param name="incomingPrefix">Icoming pattern prefix</param>
        /// <param name="searchModel">Search model</param>
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Current RPM</returns>
        [Time]
        public async Task<decimal> GetRpmAsync<TSupplierType, TSearchModel>(string incomingPrefix, TSearchModel searchModel, TSupplierType supplierType, CancellationToken cancellationToken = default)
        {
            try
            {
                incomingPrefix.NotNullOrWhiteSpace(nameof(incomingPrefix));
                supplierType.NotNull(nameof(supplierType));
                searchModel.NotNull(nameof(searchModel));

                var strSupplierType = supplierType.ConvertTo<string>();

                Logger.SetProperty(LogConstants.SearchModel, searchModel, true);
                Logger.SetProperty(LogConstants.SupplierType, strSupplierType);

                var cacheSetting = await CacheSettingService.GetFromCacheBySupplierTypeAsync(strSupplierType, cancellationToken).ConfigureAwait(false);

                var count = await CacheManager.GetCountAsync(incomingPrefix, cancellationToken).ConfigureAwait(false);
                var rpm = (decimal)count / cacheSetting.RpmDurationMinutes;

                Logger.SetProperty(LogConstants.CurrentRPM, rpm);
                return rpm;
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(GetRpmAsync)))
            {
                throw;
            }
        }

        /// <summary>
        /// Specifies whether or not the limitation is reached
        /// </summary>
        /// <typeparam name="TSupplierType">Type of SupplierType</typeparam>
        /// <param name="outgoingPrefix">Outgoing pattern prefix</param>
        /// <param name="searchModel">Search model</param>
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Return true if limitation is reached</returns>
        [Time]
        public async Task<bool> IsLimitationReachedAsync<TSupplierType, TSearchModel>(string outgoingPrefix, TSearchModel searchModel, TSupplierType supplierType, CancellationToken cancellationToken = default)
        {
            try
            {
                outgoingPrefix.NotNullOrWhiteSpace(nameof(outgoingPrefix));
                supplierType.NotNull(nameof(supplierType));
                searchModel.NotNull(nameof(searchModel));

                var strSupplierType = supplierType.ConvertTo<string>();

                Logger.SetProperty(LogConstants.SearchModel, searchModel, true);
                Logger.SetProperty(LogConstants.SupplierType, strSupplierType);

                var limitSettings = await LimitSettingService.GetFromCacheBySupplierTypeAsync(strSupplierType, cancellationToken).ConfigureAwait(false);

                foreach (var limitSetting in limitSettings)
                {
                    var prefix = outgoingPrefix + $"{limitSetting.Id}_";
                    var count = CacheManager.GetCount(prefix);

                    var isLimitationReached = count >= limitSetting.RequestLimit;
                    if (isLimitationReached)
                    {
                        Logger.SetProperty(LogConstants.IsLimitationReached, true);
                        Logger.SetProperty(LogConstants.OutgoingRequestCount, count);
                        Logger.SetProperty(LogConstants.LimitSetting, limitSetting, true);
                        return true;
                    }
                }

                Logger.SetProperty(LogConstants.IsLimitationReached, false);
                return false;
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(IsLimitationReachedAsync)))
            {
                throw;
            }
        }
    }
}
