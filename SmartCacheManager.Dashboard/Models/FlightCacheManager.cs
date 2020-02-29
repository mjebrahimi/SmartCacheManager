using SmartCacheManager.Caching;
using SmartCacheManager.Data;
using SmartCacheManager.Logging;
using SmartCacheManager.Services;
using SmartCacheManager.Utilities;
using System;
using System.Linq;

namespace SmartCacheManager.Dashboard
{
    public class FlightSearchModel
    {
#pragma warning disable CA1819 // Properties should not return arrays
        public string[] Origins { get; set; }

        public string[] Destinations { get; set; }

        public DateTime[] DepartureDates { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        public int Adult { get; set; }

        public int Child { get; set; }

        public int Infant { get; set; }
    }

    /// <summary>
    /// Service for manager flight caching
    /// </summary>
    public class FlightCacheManager : SmartCacheManager<FlightSearchModel>
    {
        public FlightCacheManager(ICacheManager cacheManager, ILoggerFactory loggerFactory, ISystemClock systemClock,
            ICacheSettingService<CacheSetting> cacheSettingService, ISearchHistoryService<CacheSetting, LimitSetting> searchHistoryService)
            : base(cacheManager, loggerFactory, systemClock, cacheSettingService, searchHistoryService)
        {
        }

        protected override string GenerateSearchResultKey(FlightSearchModel searchModel)
        {
            var departureDates = string.Join("|", searchModel.DepartureDates.Select(p => p.ToString("yyyy-MM-dd")));
            var origins = string.Join("|", searchModel.Origins);
            var destinations = string.Join("|", searchModel.Destinations);
            return $"{origins}-{destinations}-{departureDates}-{searchModel.Adult}-{searchModel.Child}-{searchModel.Infant}";
        }

        protected override string GenerateSearchHistoryKey(FlightSearchModel searchModel)
        {
            var departureDates = string.Join("|", searchModel.DepartureDates.Select(p => p.ToString("yyyy-MM-dd")));
            var origins = string.Join("|", searchModel.Origins);
            var destinations = string.Join("|", searchModel.Destinations);
            return $"{origins}-{destinations}-{departureDates}";
        }

        protected override DateTime GetSearchDate(FlightSearchModel searchModel)
        {
            return searchModel.DepartureDates.Min();
        }
    }
}
