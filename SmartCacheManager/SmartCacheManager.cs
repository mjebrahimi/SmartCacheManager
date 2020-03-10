using SmartCacheManager.Caching;
using SmartCacheManager.Data;
using SmartCacheManager.Services;
using SmartCacheManager.Utilities;
using MethodTimer;
using System;
using System.Threading;
using System.Threading.Tasks;
using SmartCacheManager.Logging;

namespace SmartCacheManager
{
    /// <summary>
    /// Service for manage smart caching
    /// </summary>
    /// <typeparam name="TSearchModel"></typeparam>
    public abstract class SmartCacheManager<TSearchModel> : SmartCacheManager<TSearchModel, CacheSetting, LimitSetting>
    {
        protected SmartCacheManager(ICacheManager cacheManager, ILoggerFactory loggerFactory, ISystemClock systemClock,
            ICacheSettingService<CacheSetting> cacheSettingService, ISearchHistoryService<CacheSetting, LimitSetting> searchHistoryService)
            : base(cacheManager, loggerFactory, systemClock, cacheSettingService, searchHistoryService)
        {
        }
    }

    /// <summary>
    /// Service for manage smart caching
    /// </summary>
    /// <typeparam name="TCacheSetting">Type of CacheSetting</typeparam>
    /// <typeparam name="TLimitSetting">Type of LimitSetting</typeparam>
    public abstract class SmartCacheManager<TSearchModel, TCacheSetting, TLimitSetting>
        where TCacheSetting : CacheSetting, new()
        where TLimitSetting : LimitSetting, new()
    {
        protected readonly ICacheManager CacheManager;
        protected readonly ILogger Logger;
        protected readonly ICacheSettingService<TCacheSetting> CacheSettingService;
        protected readonly ISearchHistoryService<TCacheSetting, TLimitSetting> SearchHistoryService;
        protected readonly ISystemClock SystemClock;

        //{0}:GetType().FullName - {1}:SupplierType
        private const string SEARCH_HISTORY_CACHE_KEY = "SmartCacheManager_SearchHistory_{0}_{1}_";
        private const string SEARCH_RESULT_CACHE_KEY = "SmartCacheManager_SearchResult_{0}_{1}_";
        private readonly string _fullName;// = typeof(BaseCacheManager<TCacheSetting, TLimitSetting>).FullName;

        protected SmartCacheManager(ICacheManager cacheManager, ILoggerFactory loggerFactory, ISystemClock systemClock,
            ICacheSettingService<TCacheSetting> cacheSettingService, ISearchHistoryService<TCacheSetting, TLimitSetting> searchHistoryService)
        {
            //Including full name of class for preventing conflicts concrete inheritance
            _fullName = GetType().FullName;

            CacheManager = cacheManager.NotNull(nameof(cacheManager));
            Logger = loggerFactory.NotNull(nameof(loggerFactory)).CreateLogger(GetType()).NotNull(nameof(Logger));
            CacheSettingService = cacheSettingService.NotNull(nameof(cacheSettingService));
            SearchHistoryService = searchHistoryService.NotNull(nameof(searchHistoryService));
            SystemClock = systemClock.NotNull(nameof(systemClock));
        }

        /// <summary>
        /// Read search result from cache (if exists) or retrive it form dataRetriever then sets
        /// گرفتن نتیجه جستجو از کش یا محاسبه مجدد آن از وب سرویس
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <param name="searchModel">Search model</param>
        /// <param name="supplierType">Supplier type</param>
        /// <param name="dataRetriever">Function to retrive search result</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Result of search</returns>
        [Time]
        public virtual async Task<TResult> SearchFromCacheAsync<TSupplierType, TResult>(TSearchModel searchModel, TSupplierType supplierType,
            Func<Task<TResult>> dataRetriever, CancellationToken cancellationToken = default)
        {
            try
            {
                using (Logger.BeginLogScopeInformation("A search process was performed"))
                {
                    Logger.SetProperty(LogConstants.LogCache, null);

                    searchModel.NotNull(nameof(searchModel));
                    supplierType.NotNull(nameof(supplierType));
                    dataRetriever.NotNull(nameof(dataRetriever));

                    Logger.SetProperty(LogConstants.SearchModel, searchModel, true);
                    Logger.SetProperty(LogConstants.SupplierType, supplierType.ConvertTo<string>());

                    var cacheKey = GetSearchResultCacheKey(searchModel, supplierType);
                    var incomingPattern = GetIncomingRequestPattern(searchModel, supplierType);
                    var outgoingPattern = GetOutgoingRequestPattern(searchModel, supplierType);

                    await SearchHistoryService.AddIncommingAsync(incomingPattern, searchModel, supplierType, cancellationToken).ConfigureAwait(false);

                    var cacheMinutes = await CalculateCacheMinutesAsync(incomingPattern, searchModel, supplierType, cancellationToken).ConfigureAwait(false);

                    await ChangeCacheMinutesAsync(cacheKey, cacheMinutes, cancellationToken).ConfigureAwait(false);

                    var limitationReached = await SearchHistoryService.IsLimitationReachedAsync(outgoingPattern, searchModel, supplierType, cancellationToken).ConfigureAwait(false);
                    Logger.SetProperty(LogConstants.IsLimitationReached, limitationReached);

                    TResult result;
                    if (limitationReached)
                    {
                        result = await CacheManager.GetAsync<TResult>(cacheKey, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        async Task<TResult> RetrieverAndLogHistroy()
                        {
                            var data = await dataRetriever().ConfigureAwait(false);
                            await SearchHistoryService.AddOutgoingAsync(outgoingPattern, searchModel, supplierType, cancellationToken).ConfigureAwait(false);
                            return data;
                        }

                        if (cacheMinutes == 0)
                            result = await RetrieverAndLogHistroy().ConfigureAwait(false);
                        else
                            result = await CacheManager.GetAsync(cacheKey, RetrieverAndLogHistroy, cacheMinutes, cancellationToken).ConfigureAwait(false);
                    }
                    return result;
                }
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(SearchFromCacheAsync)))
            {
                throw;
            }
        }

        /// <summary>
        /// Calculate finaly cache time in minutes
        /// محاسبه زمان نهایی کش به دقیقه
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Cache time in minutes</returns>
        public virtual Task<int> CalculateCacheMinutesAsync<TSupplierType>(TSearchModel searchModel, TSupplierType supplierType, CancellationToken cancellationToken = default)
        {
            try
            {
                searchModel.NotNull(nameof(searchModel));
                supplierType.NotNull(nameof(supplierType));

                var incomingPattern = GetIncomingRequestPattern(searchModel, supplierType);

                return CalculateCacheMinutesAsync(incomingPattern, searchModel, supplierType, cancellationToken);
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(CalculateCacheMinutesAsync)))
            {
                throw;
            }
        }

        /// <summary>
        /// Calulate Rpm of recent searches
        /// محسابه تعدد سرچ های اخیر
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Rpm of search</returns>
        public virtual Task<decimal> GetRpmAsync<TSupplierType>(TSearchModel searchModel, TSupplierType supplierType, CancellationToken cancellationToken = default)
        {
            try
            {
                searchModel.NotNull(nameof(searchModel));
                supplierType.NotNull(nameof(supplierType));

                var incomingPattern = GetIncomingRequestPattern(searchModel, supplierType);

                return GetRpmAsync(incomingPattern, searchModel, supplierType, cancellationToken);
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(GetRpmAsync)))
            {
                throw;
            }
        }

        /// <summary>
        /// Get current expiration based on specified search model and supplier type
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Current expiration time in TimeSpan</returns>
        public virtual Task<TimeSpan> GetExpirationAsync<TSupplierType>(TSearchModel searchModel, TSupplierType supplierType, CancellationToken cancellationToken = default)
        {
            try
            {
                searchModel.NotNull(nameof(searchModel));
                supplierType.NotNull(nameof(supplierType));

                var cacheKey = GetSearchResultCacheKey(searchModel, supplierType);

                return GetExpirationAsync(cacheKey, cancellationToken);
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(GetExpirationAsync)))
            {
                throw;
            }
        }

        /// <summary>
        /// Change or reset (if needed) cache time
        /// تغییر زمان منقضی شدن یک کش یا ریست کردن آن در صورت لزوم
        /// </summary>
        /// <param name="cacheMinutes">Calculated time to cache in minutes</param>
        /// <param name="cacheMinutes">Cache minutes</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        [Time]
        protected virtual async Task ChangeCacheMinutesAsync(string cacheKey, int cacheMinutes, CancellationToken cancellationToken = default)
        {
            try
            {
                var expiration = await GetExpirationAsync(cacheKey, cancellationToken).ConfigureAwait(false);

                if (cacheMinutes > 0 && cacheMinutes < Convert.ToDecimal(expiration.TotalMinutes))
                {
                    //Change cache time
                    await CacheManager.SetExpirationAsync(cacheKey, cacheMinutes, cancellationToken).ConfigureAwait(false);

                    Logger.SetProperty(LogConstants.CurrentExpirationChanged, true);

                    ////3 times to try
                    //if (await TrySet().ConfigureAwait(false) == false)
                    //    if (await TrySet().ConfigureAwait(false) == false)
                    //        if (await TrySet().ConfigureAwait(false) == false)
                    //            throw new Exception($"Faild to set cache {nameof(ChangeOrResetCacheMinutesAsync)}");
                    //Task<bool> TrySet() => CacheManager.TrySetAsync(key, cacheResult, cacheMinutes, cancellationToken);
                }
                else
                {
                    Logger.SetProperty(LogConstants.CurrentExpirationChanged, false);
                }
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(ChangeCacheMinutesAsync)))
            {
                throw;
            }
        }

        /// <summary>
        /// Get current expiration of specified cache key
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Current expiration time in TimeSpan</returns>
        [Time]
        protected virtual async Task<TimeSpan> GetExpirationAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var expiration = await CacheManager.GetExpirationAsync(cacheKey, cancellationToken).ConfigureAwait(false);
                Logger.SetProperty(LogConstants.CurrentExpirationMinutes, Convert.ToDecimal(expiration.TotalMinutes));
                return expiration;
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(GetExpirationAsync)))
            {
                throw;
            }
        }

        /// <summary>
        /// Calculate finaly cache time in minutes
        /// محاسبه زمان نهایی کش به دقیقه
        /// </summary>
        /// <param name="incomingPattern">Icoming pattern pattern</param>
        /// <param name="searchModel">Search model</param>
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Cache time in minutes</returns>
        [Time]
        protected virtual async Task<int> CalculateCacheMinutesAsync<TSupplierType>(string incomingPattern, TSearchModel searchModel, TSupplierType supplierType, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheSetting = await CacheSettingService.GetFromCacheBySupplierTypeAsync(supplierType, cancellationToken).ConfigureAwait(false);

                var currentRPM = await GetRpmAsync(incomingPattern, searchModel, supplierType, cancellationToken).ConfigureAwait(false);

                var searchDate = GetSearchDate(searchModel);
                Logger.SetProperty(LogConstants.SearchDate, searchDate);

                var cacheMinutes = CalculateCacheMinutesBySearchDate(searchDate, cacheSetting);

                return CalculateCacheMinutesByRPM(cacheMinutes, currentRPM, cacheSetting);
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(CalculateCacheMinutesAsync)))
            {
                throw;
            }
        }

        /// <summary>
        /// Get current Rpm based on specified pattern and supplier type
        /// </summary>
        /// <typeparam name="TSupplierType">Type of SupplierType</typeparam>
        /// <param name="incomingPattern">Icoming pattern pattern</param>
        /// <param name="searchModel">Search model</param>
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Current Rpm</returns>
        protected virtual Task<decimal> GetRpmAsync<TSupplierType>(string incomingPattern, TSearchModel searchModel, TSupplierType supplierType, CancellationToken cancellationToken = default)
        {
            try
            {
                return SearchHistoryService.GetRpmAsync(incomingPattern, searchModel, supplierType, cancellationToken);
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(GetRpmAsync)))
            {
                throw;
            }
        }

        /// <summary>
        /// Calculate cache time (in minutes) by Rpm of recent searches
        /// محاسبه زمان کش به دقیقه بر حسب تعدد جستجو های اخیر
        /// </summary>
        /// <param name="cacheMinutes">Calculated time to cache in minutes</param>
        /// <param name="currentRPM">Rpm of recent searches</param>
        /// <param name="cacheSetting">Setting for cache</param>
        /// <returns>Calculated cache time in minutes</returns>
        [Time]
        protected virtual int CalculateCacheMinutesByRPM(int cacheMinutes, decimal currentRPM, TCacheSetting cacheSetting)
        {
            try
            {
                var cacheMinutesByRpm = Calculate();
                Logger.SetProperty(LogConstants.CacheMinutesByCurrentRPM, cacheMinutesByRpm);

                return cacheMinutesByRpm;

                int Calculate()
                {
                    if (cacheMinutes == 0) return 0;

                    //Calculate the formula by two points
                    var yDiff = (decimal)cacheMinutes - cacheSetting.MinCacheMinutes;
                    var xDiff = cacheSetting.RecentSearchMinimumRPM - cacheSetting.RecentSearchMaxmimumRPM;
                    var slope = yDiff / xDiff; //شیب خط
                    var yPoint = cacheMinutes - (slope * cacheSetting.RecentSearchMinimumRPM); //نقطه شیب

                    decimal formula(decimal x) => (slope * x) + yPoint;

                    var result = Convert.ToInt32(formula(currentRPM));

                    switch (result)
                    {
                        case 0: return 0;
                        case int value when value > cacheSetting.MaxCacheMinutes:
                            return cacheSetting.MaxCacheMinutes;
                        case int value when value < cacheSetting.MinCacheMinutes:
                            return cacheSetting.MinCacheMinutes;
                        default:
                            return result;
                    }
                }
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(CalculateCacheMinutesByRPM)))
            {
                throw;
            }
        }

        /// <summary>
        /// Calculate cache time (in minutes) by date of search
        /// محاسبه زمان کش به دقیقه بر حسب تفاوت تاریخ جستجو
        /// </summary>
        /// <param name="searchDate">Date of search</param>
        /// <param name="cacheSetting">Setting for cache</param>
        /// <returns>Cache time in minutes</returns>
        [Time]
        protected virtual int CalculateCacheMinutesBySearchDate(DateTime searchDate, TCacheSetting cacheSetting)
        {
            try
            {
                var result = Calculate();
                Logger.SetProperty(LogConstants.CacheMinutesBySearchDate, result);

                return result;

                int Calculate()
                {
                    var now = SystemClock.DateTimeNow;
                    var diffHours = Convert.ToDecimal(searchDate.Subtract(now).TotalHours);

                    Logger.SetProperty(LogConstants.NowDateTime, now);
                    Logger.SetProperty(LogConstants.SearchDiffHours, diffHours);

                    if (diffHours < 0)
                        throw new ArgumentException($"Argument {nameof(searchDate)} should not be past");

                    if (diffHours <= cacheSetting.MinSearchDiffHours)
                        return cacheSetting.MinCacheMinutes;

                    if (diffHours > cacheSetting.OverSearchDiffHours)
                        return 0;

                    if (diffHours >= cacheSetting.MaxSearchDiffHours)
                        return cacheSetting.MaxCacheMinutes;

                    //Calculate the formula by two points
                    var yDiff = cacheSetting.MaxCacheMinutes - cacheSetting.MinCacheMinutes;
                    var xDiff = cacheSetting.MaxSearchDiffHours - cacheSetting.MinSearchDiffHours;
                    var slope = (decimal)yDiff / xDiff; //شیب خط
                    var yPoint = cacheSetting.MinCacheMinutes - (slope * cacheSetting.MinSearchDiffHours); //نقطه شیب

                    decimal formula(decimal x) => (slope * x) + yPoint;

                    var cacheMinutes = formula(diffHours);

                    return Convert.ToInt32(cacheMinutes);
                }
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(CalculateCacheMinutesBySearchDate)))
            {
                throw;
            }
        }

        protected virtual string GetSearchResultCacheKey<TSupplierType>(TSearchModel searchModel, TSupplierType supplierType)
        {
            try
            {
                var result = string.Format(SEARCH_RESULT_CACHE_KEY, _fullName, supplierType.ConvertTo<string>()) + GenerateSearchResultKey(searchModel);
                Logger.SetProperty(LogConstants.CacheKey, result);
                return result;
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(GetSearchResultCacheKey)))
            {
                throw;
            }
        }

        protected virtual string GetIncomingRequestPattern<TSupplierType>(TSearchModel searchModel, TSupplierType supplierType)
        {
            try
            {
                var result = string.Format(SEARCH_HISTORY_CACHE_KEY, _fullName, supplierType.ConvertTo<string>()) + GenerateSearchHistoryKey(searchModel);
                Logger.SetProperty(LogConstants.IncomingPattern, result);
                return result;
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(GetIncomingRequestPattern)))
            {
                throw;
            }
        }

        protected virtual string GetOutgoingRequestPattern<TSupplierType>(TSearchModel searchModel, TSupplierType supplierType)
        {
            try
            {
                var result = string.Format(SEARCH_HISTORY_CACHE_KEY, _fullName, supplierType.ConvertTo<string>());
                Logger.SetProperty(LogConstants.OutgoingPattern, result);
                return result;
            }
            catch (Exception ex)
            when (Logger.LogErrorIfNotBefore(ex, "Exception ocurred in {MethodName}", nameof(GetOutgoingRequestPattern)))
            {
                throw;
            }
        }

        /// <summary>
        /// Generate result cache key based on specified search model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Cache key</returns>
        protected abstract string GenerateSearchResultKey(TSearchModel searchModel);

        /// <summary>
        /// Generate history cache key based on specified search model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Cache key</returns>
        protected abstract string GenerateSearchHistoryKey(TSearchModel searchModel);

        /// <summary>
        /// Get search date based on specified search model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Date of search</returns>
        protected abstract DateTime GetSearchDate(TSearchModel searchModel);
    }
}
