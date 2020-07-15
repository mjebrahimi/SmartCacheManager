using SmartCacheManager.Utilities;
using EasyCaching.Core.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using MessagePack.Resolvers;

namespace SmartCacheManager.Caching.EasyCaching
{
    public static class EasyCachingConfigurationExtensions
    {
        /// <summary>
        /// Add EasyCaching services and EasyCachingOptions options to IServiceCollection
        /// </summary>
        /// <param name="services">services</param>
        /// <param name="configure">Action to configure EasyCachingOptions</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddEasyCachingCacheManager(this IServiceCollection services, Action<EasyCachingOptions> configure = null)
        {
            services.NotNull(nameof(services));

            services.AddEasyCaching(configure);

            return services.AddEasyCachingAsCacheManager();
        }

        /// <summary>
        /// Add EasyCaching services to IServiceCollection and configure that based on EasyCachingOptions (InMemory or Redis or Hybrid)
        /// </summary>
        /// <param name="services">services</param>
        /// <param name="configure">Action to configure EasyCachingOptions</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddEasyCaching(this IServiceCollection services, Action<EasyCachingOptions> configure = null)
        {
            services.NotNull(nameof(services));


            if (configure == null)
                configure = _ => { };
            services.Configure(configure);

            //using IOptions<> instead of IOptionsSnapshot<> because IOptions is Singleton while IOptionsSnapshot is scoped
            var optionsMonitor = services.BuildServiceProvider().GetRequiredService<IOptions<EasyCachingOptions>>();
            var options = optionsMonitor.Value;

            if (options.ProviderType == CachingProviderType.Disabled)
                return services;

            var compressorName = options.CompressionType.ToString();
            if (options.ProviderType == CachingProviderType.Redis)
            {
                switch (options.CompressionType)
                {
                    case CompressionType.Brotli:
                        services.AddBrotliCompressor(compressorName);
                        break;
                    case CompressionType.GZip:
                        services.AddGZipCompressor(compressorName);
                        break;
                    case CompressionType.Deflate:
                        services.AddDeflateCompressor(compressorName);
                        break;
                    case CompressionType.LZ4:
                        services.AddLZ4Compressor(compressorName);
                        break;
                }
            }

            services.AddEasyCaching(easyCachingOptions =>
            {
                switch (options.ProviderType)
                {
                    case CachingProviderType.InMemory:
                        var inMemory = "DefaultInMemory";

                        var memoryCache = easyCachingOptions.UseInMemory(config =>
                        {
                            // whether enable logging, default is false
                            config.EnableLogging = options.EnableLogging;
                            //config.DBConfig = new InMemoryCachingOptions
                            //{
                            //    // scan time, default value is 60s
                            //    ExpirationScanFrequency = 60,
                            //    // total count of cache items, default value is 10000
                            //    SizeLimit = 100
                            //};
                            //// the max random second will be added to cache's expiration, default value is 120
                            //config.MaxRdSecond = 120;
                            //// mutex key's alive time(ms), default is 5000
                            //config.LockMs = 5000;
                            //// when mutex key alive, it will sleep some time, default is 300
                            //config.SleepMs = 300;
                        }, inMemory);

                        break;
                    case CachingProviderType.Redis:
                        var redis = "DefaultRedis";
                        var msgpack = "MessagePack";

                        var redisCache = easyCachingOptions.UseRedis(config =>
                        {
                            // whether enable logging, default is false
                            config.EnableLogging = options.EnableLogging;
                            //redis database endpoint (host:port)
                            config.DBConfig.Endpoints.Add(new ServerEndPoint(options.RedisHost, options.RedisPort));
                            config.SerializerName = msgpack;
                            //Access for redis FLUSHDB command
                            config.DBConfig.AllowAdmin = options.RedisAllowAdmin;
                        }, redis);

                        //set binary serializer
                        //https://easycaching.readthedocs.io/en/latest/MessagePack/
                        redisCache.WithMessagePack(opt =>
                        {
                            opt.EnableCustomResolver = true;
                            opt.CustomResolvers = CompositeResolver.Create(
                                // This can solve DateTime time zone problem
                                NativeDateTimeResolver.Instance,
                                ContractlessStandardResolver.Instance
                            );
                        }, msgpack);

                        if (options.CompressionType != CompressionType.Disabled)
                            redisCache.WithCompressor(msgpack, compressorName);
                        break;
                    case CachingProviderType.Disabled:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(options.ProviderType), "CachingProviderType is not valid.");
                }
            });

            return services;
        }

        /// <summary>
        /// Register EasyCachingCacheManager as ICacheManager as Scoped lifetime
        /// </summary>
        /// <param name="services">services</param>
        /// <returns>IServiceCollection</returns>
        private static IServiceCollection AddEasyCachingAsCacheManager(this IServiceCollection services)
        {
            services.NotNull(nameof(services));

            services.TryAddScoped<ICacheManager, EasyCachingCacheManager>();
            return services;
        }
    }
}
