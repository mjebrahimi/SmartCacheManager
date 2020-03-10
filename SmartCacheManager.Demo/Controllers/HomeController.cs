using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartCacheManager.Caching;
using SmartCacheManager.Demo.Models;

namespace SmartCacheManager.Demo.Controllers
{
    public class CacheDetail
    {
        public decimal CurrentRPM { get; set; }
        public int CalculatedCacheMinutes { get; set; }
        public TimeSpan ExistedCacheMinutes { get; set; }
    }

    public class IndexViewModel
    {
        public FlightSearchResult FlightSearchResult { get; set; }
        public List<CacheDetail> CacheDetails { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FlightCacheManager _flightCacheManager;
        private readonly ICacheManager _cacheManager;
        private static readonly List<CacheDetail> _cacheDetails = new List<CacheDetail>();

        public HomeController(ILogger<HomeController> logger, FlightCacheManager flightCacheManager, ICacheManager cacheManager)
        {
            _logger = logger;
            _flightCacheManager = flightCacheManager;
            _cacheManager = cacheManager;
        }

        public async Task<IActionResult> Index()
        {
            var searchModel = new FlightSearchModel
            {
                Adult = 1,
                Child = 2,
                Infant = 3,
                Origins = new[] { "THR" },
                Destinations = new[] { "ISF" },
                DepartureDates = new[] { DateTime.Now.AddDays(29) }
            };

            var existed = await _flightCacheManager.GetExpirationAsync(searchModel, 1);

            var result = await _flightCacheManager.SearchFromCacheAsync(searchModel, 1, async () =>
            {
                return new FlightSearchResult();
            });

            var cacheDetail = new CacheDetail
            {
                CurrentRPM = await _flightCacheManager.GetRpmAsync(searchModel, 1),
                CalculatedCacheMinutes = await _flightCacheManager.CalculateCacheMinutesAsync(searchModel, 1),
                ExistedCacheMinutes = existed,
            };
            _cacheDetails.Insert(0, cacheDetail);

            var viewModel = new IndexViewModel
            {
                FlightSearchResult = result,
                CacheDetails = _cacheDetails
            };

            return View(viewModel);
        }

        public async Task<IActionResult> FlushCacheDb()
        {
            await _cacheManager.FlushAsync();
            _cacheDetails.Clear();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
