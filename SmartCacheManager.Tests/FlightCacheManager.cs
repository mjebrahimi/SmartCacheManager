using SmartCacheManager.Caching;
using SmartCacheManager.Data;
using SmartCacheManager.Logging;
using SmartCacheManager.Services;
using SmartCacheManager.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCacheManager.Tests
{
    public interface IFlightCacheManager
    {
        Task<TResult> SearchFromCacheAsync<TSupplierType, TResult>(FlightSearchModel searchModel, TSupplierType supplierType, Func<Task<TResult>> dataRetriever, CancellationToken cancellationToken = default);
        Task<int> CalculateCacheMinutesAsync<TSupplierType>(FlightSearchModel searchModel, TSupplierType supplierType, CancellationToken cancellationToken = default);
        Task<decimal> GetRpmAsync<TSupplierType>(FlightSearchModel searchModel, TSupplierType supplierType, CancellationToken cancellationToken = default);
        Task<TimeSpan> GetExpirationAsync<TSupplierType>(FlightSearchModel searchModel, TSupplierType supplierType, CancellationToken cancellationToken = default);
    }

    public class FlightCacheManager : SmartCacheManager<FlightSearchModel>, IFlightCacheManager
    {
        public FlightCacheManager(ICacheManager cacheManager, ILoggerFactory loggerFactory, ISystemClock systemClock,
            ICacheSettingService<CacheSetting> cacheSettingService, ISearchHistoryService<CacheSetting, LimitSetting> searchHistoryService)
            : base(cacheManager, loggerFactory, systemClock, cacheSettingService, searchHistoryService)
        {
        }

        protected override string GenerateSearchHistoryKey(FlightSearchModel searchModel)
        {
            return GenerateSearchResultKey(searchModel);
        }

        protected override string GenerateSearchResultKey(FlightSearchModel searchModel)
        {
            return $"{searchModel.Origin}-{searchModel.Destination}-{searchModel.SearchDate.ToString("yyyy-MM-dd")}";
        }

        protected override DateTime GetSearchDate(FlightSearchModel searchModel)
        {
            return searchModel.SearchDate;
        }
    }

    public class FlightSearchModel
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime SearchDate { get; set; }
    }
}
