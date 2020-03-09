using SmartCacheManager.Utilities;
using EasyCaching.Core;
using EasyCaching.Core.Configurations;
using EasyCaching.InMemory;
using EasyCaching.Redis;
using EasyCaching.Serialization.Json;
using EasyCaching.Serialization.MessagePack;
using EasyCaching.Serialization.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using MessagePack.Resolvers;
using EasyCaching.Core.Serialization;

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

            var inMemory = "DefaultInMemory";
            var redis = "DefaultRedis";

            if (configure == null)
                configure = _ => { };
            services.Configure(configure);

            //using IOptions<> instead of IOptionsSnapshot<> because IOptions is Singleton while IOptionsSnapshot is scoped
            var optionsMonitor = services.BuildServiceProvider().GetRequiredService<IOptions<EasyCachingOptions>>();
            var options = optionsMonitor.Value;

            if (options.ProviderType == CachingProviderType.Disabled)
                return services;

            services.AddEasyCaching(easyCachingOptions =>
            {
                if (options.ProviderType == CachingProviderType.InMemory || options.ProviderType == CachingProviderType.Hybrid)
                {
                    //use memory cache that named inMemory
                    easyCachingOptions.UseInMemory(config =>
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
                }

                if (options.ProviderType == CachingProviderType.Redis || options.ProviderType == CachingProviderType.Hybrid)
                {
                    var serializerName = options.BinarySerializerType.ToString();

                    //use redis cache that named redis
                    var redisCache = easyCachingOptions.UseRedis(config =>
                    {
                        // whether enable logging, default is false
                        config.EnableLogging = options.EnableLogging;
                        //redis database endpoint (host:port)
                        config.DBConfig.Endpoints.Add(new ServerEndPoint(options.RedisHost, options.RedisPort));
                        config.SerializerName = serializerName;
                        //Access for redis FLUSHDB command
                        config.DBConfig.AllowAdmin = options.RedisAllowAdmin;
                    }, redis);

                    //set binary serializer (MessagePack or Protobuf or Json)
                    switch (options.BinarySerializerType)
                    {
                        case BinarySerializerType.MessagePack:
                            //https://easycaching.readthedocs.io/en/latest/MessagePack/
                            //CompositeResolver.RegisterAndSetAsDefault(
                            //    // This can solve DateTime time zone problem
                            //    NativeDateTimeResolver.Instance,
                            //    ContractlessStandardResolver.Instance
                            //);
                            //redisCache.WithMessagePack(opt =>
                            //{
                            //    opt.EnableCustomResolver = true;
                            //}, msgPack);
                            redisCache.WithMessagePack(serializerName);
                            break;
                        case BinarySerializerType.Protobuf:
                            redisCache.WithProtobuf(serializerName);
                            break;
                        case BinarySerializerType.Json:
                            redisCache.WithJson(serializerName);
                            break;
                    }
                }

                if (options.ProviderType == CachingProviderType.Hybrid)
                {
                    //Required API in CacheManager dose not support in EasyCaching Hybrid mode
                    throw new NotSupportedException();

                    //https://easycaching.readthedocs.io/en/latest/Hybrid
                    //easyCachingOptions.UseHybrid(config =>
                    //{
                    //    config.EnableLogging = cacheSettings.EnableLogging;
                    //    config.TopicName = topicName;
                    //    config.LocalCacheProviderName = inMemory;
                    //    config.DistributedCacheProviderName = redis;
                    //}, hybrid)
                    ////Bus.Redis or Bus.RabbitMQ
                    //.WithRedisBus(busConf =>
                    //{
                    //    busConf.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
                    //});
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


        /// <summary>
        /// WORKAROUND: fixing the time zone lose problem of DateTime in version of <= v0.8.0 of EasyCaching.Serialization.MessagePack
        /// https://easycaching.readthedocs.io/en/latest/MessagePack/
        /// </summary>
        /// <param name="serviceProvider">serviceProvider</param>
        public static void Fix_EasyCaching_MessagePack_TimeZone_LoseProblem(this IServiceProvider serviceProvider)
        {
            serviceProvider.NotNull(nameof(serviceProvider));
            using (var scope = serviceProvider.CreateScope())
            {
                var serializer = scope.ServiceProvider.GetService<IEasyCachingSerializer>();
                if (serializer is DefaultMessagePackSerializer)
                {
                    if (serializer.GetType().Assembly.GetName().Version <= new Version("0.8.0"))
                    {
                        CompositeResolver.RegisterAndSetAsDefault(
                            // This can solve DateTime time zone problem
                            NativeDateTimeResolver.Instance,
                            ContractlessStandardResolver.Instance
                        );
                    }
                }
            }
        }
    }
}
