using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCacheManager.Caching
{
    /// <summary>
    /// Caching abstraction
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// Whether the cached value contains the specified cacheKey.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <returns>The exists.</returns>
        bool Exists(string cacheKey);

        /// <summary>
        /// Whether the cached value contains the specified cacheKey async.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>The exists.</returns>
        Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the specified cacheKey, dataRetriever and expiration.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <returns>Cached value</returns>
        T Get<T>(string cacheKey);

        /// <summary>
        /// Gets the specified cacheKey, dataRetriever and expiration.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="dataRetriever">Function to retrive data</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        /// <returns>Cached value</returns>
        T Get<T>(string cacheKey, Func<T> dataRetriever, int? expirationMinutes = null);

        /// <summary>
        /// Gets the specified cacheKey, dataRetriever and expiration async.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Cached value</returns>
        Task<T> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the specified cacheKey, dataRetriever and expiration async.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="dataRetriever">Function to retrive data</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Cached value</returns>
        Task<T> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, int? expirationMinutes = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the by prefix.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="prefix">Prefix of CacheKey.</param>
        /// <returns>Dictionary of key/value cache items</returns>
        Dictionary<string, T> GetByPrefix<T>(string prefix);

        /// <summary>
        /// Gets the by prefix async.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="prefix">Prefix of CacheKey.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Dictionary of key/value cache items</returns>
        Task<Dictionary<string, T>> GetByPrefixAsync<T>(string prefix, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        /// <returns>Count</returns>
        int GetCount(string prefix = "");

        /// <summary>
        /// Gets the count async.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Count</returns>
        Task<int> GetCountAsync(string prefix = "", CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the exporation of specify cachekey async.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <returns>Expiration in TimeSpan</returns>
        TimeSpan GetExpiration(string cacheKey);

        /// <summary>
        /// Gets the exporation of specify cachekey async.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <returns>Expiration in TimeSpan</returns>
        /// <param name="cancellationToken">cancellationToken</param>
        Task<TimeSpan> GetExpirationAsync(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Set the the exporation of specify cachekey.
        /// </summary>>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        void SetExpiration(string cacheKey, int? expirationMinutes = null);

        /// <summary>
        /// Set the the exporation of specify cachekey async.
        /// </summary>>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        Task SetExpirationAsync(string cacheKey, int? expirationMinutes = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the specified cacheKey.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        void Remove(string cacheKey);

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        void RemoveAll(IEnumerable<string> cacheKeys);

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        Task RemoveAllAsync(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes cached item by cachekey's prefix.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        void RemoveByPrefix(string prefix);

        /// <summary>
        /// Removes cached item by cachekey's prefix async.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

        /// <summary>
        /// Set the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="cacheValue">Cache value</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        void Set<T>(string cacheKey, T cacheValue, int? expirationMinutes = null);

        /// <summary>
        /// Sets all.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="value">Dictionary of key/value cache items</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        void SetAll<T>(IDictionary<string, T> value, int? expirationMinutes = null);

        /// <summary>
        /// Sets all async.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="value">Dictionary of key/value cache items</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        Task SetAllAsync<T>(IDictionary<string, T> value, int? expirationMinutes = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the specified cacheKey, cacheValue and expiration async.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="cacheValue">Cache value</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        Task SetAsync<T>(string cacheKey, T cacheValue, int? expirationMinutes = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tries the set.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="cacheValue">Cache value</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        /// <returns>Determines whether it is successful</returns>
        bool TrySet<T>(string cacheKey, T cacheValue, int? expirationMinutes = null);

        /// <summary>
        /// Tries the set async.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="cacheValue">Cache value</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Determines whether it is successful</returns>
        Task<bool> TrySetAsync<T>(string cacheKey, T cacheValue, int? expirationMinutes = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Increments the number stored at key by value async. 
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="value">Increment value</param>
        /// <returns>Increased value</returns>
        long IncrBy(string cacheKey, long value = 1);

        /// <summary>
        /// Increments the number stored at key by value. 
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="value">Increment value</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Increased value</returns>
        Task<long> IncrByAsync(string cacheKey, long value = 1, CancellationToken cancellationToken = default);

        /// <summary>
        /// Flush All Cached Item
        /// </summary>
        void Flush();

        /// <summary>
        /// Flush All Cached Item async.
        /// </summary>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        Task FlushAsync(CancellationToken cancellationToken = default);
    }
}
