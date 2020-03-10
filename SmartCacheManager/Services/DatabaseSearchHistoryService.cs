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
    /// Database implementation of ISearchHistoryService
    /// </summary>
    /// <typeparam name="TCacheSetting">Type of CacheSetting</typeparam>
    /// <typeparam name="TLimitSetting">Type of LimitSetting</typeparam>
    public class DatabaseSearchHistoryService<TCacheSetting, TLimitSetting> : ISearchHistoryService<TCacheSetting, TLimitSetting>
        where TCacheSetting : CacheSetting, new()
        where TLimitSetting : LimitSetting, new()
    {
        protected readonly IGenericStore<IncomingRequest> IncomingRequestStore;
        protected readonly IGenericStore<OutgoingRequest> OutgoingRequestStore;
        protected readonly ICacheSettingService<TCacheSetting> CacheSettingService;
        protected readonly ILimitSettingService<TLimitSetting> LimitSettingService;
        protected readonly ILogger Logger;
        protected readonly ISystemClock SystemClock;
        protected readonly IAsyncLock AsyncLock;

        public DatabaseSearchHistoryService(IGenericStore<IncomingRequest> incomingRequestStore, IGenericStore<OutgoingRequest> outgoingRequestStore,
            ICacheSettingService<TCacheSetting> cacheSettingService, ILimitSettingService<TLimitSetting> limitSettingService,
            ILoggerFactory loggerFactory, ISystemClock systemClock, IAsyncLock asyncLock)
        {
            IncomingRequestStore = incomingRequestStore.NotNull(nameof(incomingRequestStore));
            OutgoingRequestStore = outgoingRequestStore.NotNull(nameof(outgoingRequestStore));
            CacheSettingService = cacheSettingService.NotNull(nameof(cacheSettingService));
            LimitSettingService = limitSettingService.NotNull(nameof(limitSettingService));
            Logger = loggerFactory.NotNull(nameof(loggerFactory)).CreateLogger(GetType()).NotNull(nameof(Logger));
            SystemClock = systemClock.NotNull(nameof(systemClock));
            AsyncLock = asyncLock.NotNull(nameof(asyncLock));
        }

        /// <summary>
        /// Add an incoming search request
        /// </summary>
        /// <typeparam name="TSupplierType">Type of SupplierType</typeparam>
        /// <param name="incomingPrefix">Icoming pattern prefix</param>
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        [Time]
        public async Task AddIncommingAsync<TSupplierType>(string incomingPrefix, TSupplierType supplierType, CancellationToken cancellationToken = default)
        {
            try
            {
                incomingPrefix.NotNullOrWhiteSpace(nameof(incomingPrefix));
                supplierType.NotNull(nameof(supplierType));

                var strSupplierType = supplierType.ConvertTo<string>();
                var hashCode = incomingPrefix.GetInvariantHashCode();

                Logger.SetProperty(LogConstants.IncomingRequestHashCode, hashCode);
                Logger.SetProperty(LogConstants.SupplierType, strSupplierType);

                var incomingRequest = new IncomingRequest
                {
                    CreatedAt = SystemClock.DateTimeNow,
                    HashCode = hashCode
                };

                using (await AsyncLock.LockAsync())
                    await IncomingRequestStore.AddAsync(incomingRequest, cancellationToken);
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
        /// <param name="outgoingPrefix">Outgoing pattern prefix</param>
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        [Time]
        public async Task AddOutgoingAsync<TSupplierType>(string outgoingPrefix, TSupplierType supplierType, CancellationToken cancellationToken = default)
        {
            try
            {
                outgoingPrefix.NotNullOrWhiteSpace(nameof(outgoingPrefix));
                supplierType.NotNull(nameof(supplierType));

                var strSupplierType = supplierType.ConvertTo<string>();
                var hashCode = outgoingPrefix.GetInvariantHashCode();

                Logger.SetProperty(LogConstants.OutgoingRequestHashCode, hashCode);
                Logger.SetProperty(LogConstants.SupplierType, strSupplierType);

                var outgoingRequest = new OutgoingRequest
                {
                    CreatedAt = SystemClock.DateTimeNow,
                    HashCode = hashCode
                };

                using (await AsyncLock.LockAsync())
                    await OutgoingRequestStore.AddAsync(outgoingRequest, cancellationToken);
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
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Current RPM</returns>
        [Time]
        public async Task<decimal> GetRpmAsync<TSupplierType>(string incomingPrefix, TSupplierType supplierType, CancellationToken cancellationToken = default)
        {
            try
            {
                incomingPrefix.NotNullOrWhiteSpace(nameof(incomingPrefix));
                supplierType.NotNull(nameof(supplierType));

                var hashCode = incomingPrefix.GetInvariantHashCode();

                Logger.SetProperty(LogConstants.IncomingRequestHashCode, hashCode);
                Logger.SetProperty(LogConstants.SupplierType, supplierType.ConvertTo<string>());

                var cacheSetting = await CacheSettingService.GetFromCacheBySupplierTypeAsync(supplierType, cancellationToken).ConfigureAwait(false);
                var rpmDurationDateTime = SystemClock.DateTimeNow.AddMinutes(-cacheSetting.RpmDurationMinutes);

                int count;
                using (await AsyncLock.LockAsync())
                    count = await IncomingRequestStore.TableNoTracking.CountAsync(p => p.HashCode == hashCode && p.CreatedAt >= rpmDurationDateTime).ConfigureAwait(false);

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
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Return true if limitation is reached</returns>
        [Time]
        public async Task<bool> IsLimitationReachedAsync<TSupplierType>(string outgoingPrefix, TSupplierType supplierType, CancellationToken cancellationToken = default)
        {
            try
            {
                outgoingPrefix.NotNullOrWhiteSpace(nameof(outgoingPrefix));
                supplierType.NotNull(nameof(supplierType));

                var hashCode = outgoingPrefix.GetInvariantHashCode();

                Logger.SetProperty(LogConstants.OutgoingRequestHashCode, hashCode);
                Logger.SetProperty(LogConstants.SupplierType, supplierType.ConvertTo<string>());

                var limitSettings = await LimitSettingService.GetFromCacheBySupplierTypeAsync(supplierType, cancellationToken).ConfigureAwait(false);

                foreach (var limitSetting in limitSettings)
                {
                    var limitDurationDateTime = SystemClock.DateTimeNow.AddHours(-limitSetting.LimitDurationHours);

                    int count;
                    using (await AsyncLock.LockAsync())
                        count = await OutgoingRequestStore.TableNoTracking.CountAsync(p => p.HashCode == hashCode && p.CreatedAt >= limitDurationDateTime).ConfigureAwait(false);

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
