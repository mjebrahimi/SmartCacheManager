using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace SmartCacheManager.Tests
{
    public class WebServiceCacheManagerTests : TestBase
    {
        [Test]
        public async Task WebServiceCacheManager_Test()
        {
            var flightCacheManager = ServiceProvider.GetRequiredService<IFlightCacheManager>();

            var supplier = "SupplierType_1";
            var searchModel = new FlightSearchModel
            {
                Origin = "MUC",
                Destination = "HAM",
                SearchDate = DateTime.Now.AddDays(3)
            };

            var expireation1 = await flightCacheManager.GetExpirationAsync(searchModel, supplier);
            Assert.AreEqual(expireation1, TimeSpan.Zero);

            var callTimes = 0;
            var iteration = 3;
            for (int i = 0; i < iteration; i++)
            {
                var result = await flightCacheManager.SearchFromCacheAsync(searchModel, supplier, async () =>
                {
                    callTimes++;
                    return DateTime.Now;
                });
            }
            Assert.AreEqual(callTimes, 1);

            var currentRpm = await flightCacheManager.GetRpmAsync(searchModel, supplier);
            Assert.AreEqual(currentRpm, 1.5M);

            var expireation2 = await flightCacheManager.CalculateCacheMinutesAsync(searchModel, supplier);
            var expireation3 = await flightCacheManager.GetExpirationAsync(searchModel, supplier);
            Assert.AreEqual(expireation2, Convert.ToInt32(expireation3.TotalMinutes));
        }
    }
}
