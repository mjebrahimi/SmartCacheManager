using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SmartCacheManager.Caching;
using SmartCacheManager.Caching.EasyCaching;
using System;
using System.Threading.Tasks;

namespace SmartCacheManager.Tests
{
    public class TestBase
    {
        protected IServiceProvider ServiceProvider;
        protected IServiceCollection Services;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();

            services.AddSmartCacheManager(
                opt => opt.UseInMemoryDatabase("CacheManagerDb"),
                opt => opt.ProviderType = CachingProviderType.Redis,
                opt => opt.EnableSqlLog = false);

            services.AddScoped<IFlightCacheManager, FlightCacheManager>();

            Services = services;
            ServiceProvider = services.BuildServiceProvider();

            var app = new ApplicationBuilder(ServiceProvider);
            app.InitializeSmartCacheManager();
        }

        [TearDown]
        public async Task TearDown()
        {
            var cacheManager = ServiceProvider.GetRequiredService<ICacheManager>();
            await cacheManager.RemoveByPrefixAsync("SmartCacheManager");
            ((IDisposable)ServiceProvider).Dispose();
        }
    }
}