namespace SmartCacheManager.Caching.EasyCaching
{
    /// <summary>
    /// Options to configure cache manager
    /// </summary>
    public class EasyCachingOptions
    {
        /// <summary>
        /// Get or set caching provider type. Default is CachingProviderType.Redis
        /// </summary>
        public CachingProviderType ProviderType { get; set; } = CachingProviderType.Redis;

        /// <summary>
        /// Get or set redis host. Default is '127.0.0.1'
        /// </summary>
        public string RedisHost { get; set; } = "127.0.0.1";

        /// <summary>
        /// Get or set redis port. Default is 6379
        /// </summary>
        public int RedisPort { get; set; } = 6379;

        /// <summary>
        /// Indicates ADMIN mode of redis (required for redis FLUSHDB command)
        /// </summary>
        public bool RedisAllowAdmin { get; set; } = true;

        /// <summary>
        /// Get or set default cache expiration time in minutes. Default is 15
        /// </summary>
        public int DefaultCacheMinutes { get; set; } = 15;

        /// <summary>
        /// Gets or sets a value indicating whether enable logging. Default is false
        /// </summary>
        public bool EnableLogging { get; set; } = false;
    }
}
