using SmartCacheManager.Utilities;
using EasyCaching.Core;
using EasyCaching.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCacheManager.Caching.EasyCaching
{
    /// <summary>
    /// EasyCaching implementation of ICacheManager
    /// </summary>
    public class EasyCachingCacheManager : ICacheManager
    {
        private readonly EasyCachingOptions _options;
        private readonly IEasyCachingProvider _easyCachingProvider;
        private readonly IRedisCachingProvider _redisCachingProvider;
        private readonly IHybridCachingProvider _hybridCachingProvider;

        public EasyCachingCacheManager(IOptionsSnapshot<EasyCachingOptions> options, IServiceProvider serviceProvider)
        {
            serviceProvider.NotNull(nameof(serviceProvider));
            _options = options.NotNull(nameof(options)).Value;

            //Initialize cache provider based on ProviderType (InMemory/Redis or Hybrid)
            switch (_options.ProviderType)
            {
                case CachingProviderType.InMemory:
                    _easyCachingProvider = serviceProvider.GetRequiredService<IEasyCachingProvider>();
                    break;
                case CachingProviderType.Redis:
                    _easyCachingProvider = serviceProvider.GetRequiredService<IEasyCachingProvider>();
                    _redisCachingProvider = serviceProvider.GetRequiredService<IRedisCachingProvider>();
                    break;
                case CachingProviderType.Hybrid:
                    _hybridCachingProvider = serviceProvider.GetRequiredService<IHybridCachingProvider>();
                    break;
                case CachingProviderType.Disabled:
                default:
                    throw new InvalidOperationException("Caching is disabled.");
            }
        }

        /// <summary>
        /// Whether the cached value contains the specified cacheKey.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <returns>The exists.</returns>
        public bool Exists(string cacheKey)
        {
            if (_options.ProviderType == CachingProviderType.Hybrid)
                return _hybridCachingProvider.Exists(cacheKey);
            return _easyCachingProvider.Exists(cacheKey);
        }

        /// <summary>
        /// Whether the cached value contains the specified cacheKey async.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>The exists.</returns>
        public Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            if (_options.ProviderType == CachingProviderType.Hybrid)
                return _hybridCachingProvider.ExistsAsync(cacheKey);
            return _easyCachingProvider.ExistsAsync(cacheKey);
        }

        /// <summary>
        /// Gets the specified cacheKey, dataRetriever and expiration.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <returns>Cached value</returns>
        public T Get<T>(string cacheKey)
        {
            if (_options.ProviderType == CachingProviderType.Hybrid)
                return _hybridCachingProvider.Get<T>(cacheKey).Value;
            return _easyCachingProvider.Get<T>(cacheKey).Value;
        }

        /// <summary>
        /// Gets the specified cacheKey, dataRetriever and expiration.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="dataRetriever">Function to retrive data</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        /// <returns>Cached value</returns>
        public T Get<T>(string cacheKey, Func<T> dataRetriever, int? expirationMinutes = null)
        {
            var timespan = TimeSpan.FromMinutes(expirationMinutes ?? _options.DefaultCacheMinutes);
            if (_options.ProviderType == CachingProviderType.Hybrid)
                return _hybridCachingProvider.Get(cacheKey, dataRetriever, timespan).Value;
            return _easyCachingProvider.Get(cacheKey, dataRetriever, timespan).Value;
        }

        /// <summary>
        /// Gets the specified cacheKey, dataRetriever and expiration async.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Cached value</returns>
        public async Task<T> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            if (_options.ProviderType == CachingProviderType.Hybrid)
                return (await _hybridCachingProvider.GetAsync<T>(cacheKey).ConfigureAwait(false)).Value;
            return (await _easyCachingProvider.GetAsync<T>(cacheKey).ConfigureAwait(false)).Value;
        }

        /// <summary>
        /// Gets the specified cacheKey, dataRetriever and expiration async.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="dataRetriever">Function to retrive data</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Cached value</returns>
        public async Task<T> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, int? expirationMinutes = null, CancellationToken cancellationToken = default)
        {
            var timespan = TimeSpan.FromMinutes(expirationMinutes ?? _options.DefaultCacheMinutes);
            if (_options.ProviderType == CachingProviderType.Hybrid)
                return (await _hybridCachingProvider.GetAsync(cacheKey, dataRetriever, timespan).ConfigureAwait(false)).Value;
            return (await _easyCachingProvider.GetAsync(cacheKey, dataRetriever, timespan).ConfigureAwait(false)).Value;
        }

        /// <summary>
        /// Gets the by prefix.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="prefix">Prefix of CacheKey.</param>
        /// <returns>Dictionary of key/value cache items</returns>
        public Dictionary<string, T> GetByPrefix<T>(string prefix)
        {
            if (_options.ProviderType == CachingProviderType.Hybrid)
                throw new NotSupportedException($"{nameof(IHybridCachingProvider)} dose not support {nameof(GetByPrefix)} method");
            var items = _easyCachingProvider.GetByPrefix<T>(prefix);
            return items.ToDictionary(p => p.Key, p => p.Value.Value);
        }

        /// <summary>
        /// Gets the by prefix async.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="prefix">Prefix of CacheKey.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Dictionary of key/value cache items</returns>
        public async Task<Dictionary<string, T>> GetByPrefixAsync<T>(string prefix, CancellationToken cancellationToken = default)
        {
            if (_options.ProviderType == CachingProviderType.Hybrid)
                throw new NotSupportedException($"{nameof(IHybridCachingProvider)} dose not support {nameof(GetByPrefixAsync)} method");
            var items = await _easyCachingProvider.GetByPrefixAsync<T>(prefix).ConfigureAwait(false);
            return items.ToDictionary(p => p.Key, p => p.Value.Value);
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        /// <returns>Count</returns>
        public int GetCount(string prefix = "")
        {
            //if (_options.ProviderType == CachingProviderType.Hybrid)
            //    throw new NotSupportedException($"{nameof(IHybridCachingProvider)} dose not support {nameof(GetCount)} method");
            //

            switch (_options.ProviderType)
            {
                case CachingProviderType.InMemory:
                    return _easyCachingProvider.GetCount(prefix);
                case CachingProviderType.Redis:
                    return GetCountFast(prefix);
                default:
                    throw new NotSupportedException($"{nameof(IHybridCachingProvider)} dose not support {nameof(GetCount)} method");
            }
        }

        /// <summary>
        /// Gets the count async.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Count</returns>
        public Task<int> GetCountAsync(string prefix = "", CancellationToken cancellationToken = default)
        {
            //return Task.FromResult(GetCount(prefix));

            switch (_options.ProviderType)
            {
                case CachingProviderType.InMemory:
                    return Task.FromResult(_easyCachingProvider.GetCount(prefix));
                case CachingProviderType.Redis:
                    return GetCountFastAsync(prefix);
                default:
                    throw new NotSupportedException($"{nameof(IHybridCachingProvider)} dose not support {nameof(GetCount)} method");
            }
        }

        /// <summary>
        /// Gets the exporation of specify cachekey async.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <returns>Expiration in TimeSpan</returns>
        public TimeSpan GetExpiration(string cacheKey)
        {
            if (_options.ProviderType == CachingProviderType.Hybrid)
                throw new NotSupportedException($"{nameof(IHybridCachingProvider)} dose not support {nameof(GetExpiration)} method");
            return _easyCachingProvider.GetExpiration(cacheKey);
        }

        /// <summary>
        /// Gets the exporation of specify cachekey async.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Expiration in TimeSpan</returns>
        public Task<TimeSpan> GetExpirationAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            if (_options.ProviderType == CachingProviderType.Hybrid)
                throw new NotSupportedException($"{nameof(IHybridCachingProvider)} dose not support {nameof(GetExpirationAsync)} method");
            return _easyCachingProvider.GetExpirationAsync(cacheKey);
        }

        /// <summary>
        /// Set the the exporation of specify cachekey.
        /// </summary>>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        public void SetExpiration(string cacheKey, int? expirationMinutes = null)
        {
            var timespan = TimeSpan.FromMinutes(expirationMinutes ?? _options.DefaultCacheMinutes);
            switch (_options.ProviderType)
            {
                case CachingProviderType.InMemory:
                    var cacheValue = _easyCachingProvider.Get<dynamic>(cacheKey);
                    _easyCachingProvider.Set(cacheKey, cacheValue, timespan);
                    break;
                case CachingProviderType.Redis:
                    _redisCachingProvider.KeyExpire(cacheKey, Convert.ToInt32(timespan.TotalSeconds));
                    break;
                default:
                    throw new NotSupportedException($"{nameof(IHybridCachingProvider)} dose not support {nameof(SetExpiration)} method");
            }
        }

        /// <summary>
        /// Set the the exporation of specify cachekey async.
        /// </summary>>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        public async Task SetExpirationAsync(string cacheKey, int? expirationMinutes = null, CancellationToken cancellationToken = default)
        {
            var timespan = TimeSpan.FromMinutes(expirationMinutes ?? _options.DefaultCacheMinutes);
            switch (_options.ProviderType)
            {
                case CachingProviderType.InMemory:
                    var cacheValue = _easyCachingProvider.Get<dynamic>(cacheKey);
                    _easyCachingProvider.Set(cacheKey, cacheValue, timespan);
                    break;
                case CachingProviderType.Redis:
                    await _redisCachingProvider.KeyExpireAsync(cacheKey, Convert.ToInt32(timespan.TotalSeconds));
                    break;
                default:
                    throw new NotSupportedException($"{nameof(IHybridCachingProvider)} dose not support {nameof(SetExpiration)} method");
            }
        }

        /// <summary>
        /// Removes the specified cacheKey.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        public void Remove(string cacheKey)
        {
            if (_options.ProviderType == CachingProviderType.Hybrid)
                _hybridCachingProvider.Remove(cacheKey);
            _easyCachingProvider.Remove(cacheKey);
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        public void RemoveAll(IEnumerable<string> cacheKeys)
        {
            if (_options.ProviderType == CachingProviderType.Hybrid)
                _hybridCachingProvider.RemoveAll(cacheKeys);
            _easyCachingProvider.RemoveAll(cacheKeys);
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        public Task RemoveAllAsync(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default)
        {
            if (_options.ProviderType == CachingProviderType.Hybrid)
                return _hybridCachingProvider.RemoveAllAsync(cacheKeys);
            return _easyCachingProvider.RemoveAllAsync(cacheKeys);
        }

        /// <summary>
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        public Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            if (_options.ProviderType == CachingProviderType.Hybrid)
                return _hybridCachingProvider.RemoveAsync(cacheKey);
            return _easyCachingProvider.RemoveAsync(cacheKey);
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        public void RemoveByPrefix(string prefix)
        {
            if (_options.ProviderType == CachingProviderType.Hybrid)
                _hybridCachingProvider.RemoveByPrefix(prefix);
            _easyCachingProvider.RemoveByPrefix(prefix);
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix async.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            if (_options.ProviderType == CachingProviderType.Hybrid)
                return _hybridCachingProvider.RemoveByPrefixAsync(prefix);
            return _easyCachingProvider.RemoveByPrefixAsync(prefix);
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="cacheValue">Cache value</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        public void Set<T>(string cacheKey, T cacheValue, int? expirationMinutes = null)
        {
            var timespan = TimeSpan.FromMinutes(expirationMinutes ?? _options.DefaultCacheMinutes);
            if (_options.ProviderType == CachingProviderType.Hybrid)
                _hybridCachingProvider.Set(cacheKey, cacheValue, timespan);
            _easyCachingProvider.Set(cacheKey, cacheValue, timespan);
        }

        /// <summary>
        /// Sets all.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="value">Dictionary of key/value cache items</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        public void SetAll<T>(IDictionary<string, T> value, int? expirationMinutes = null)
        {
            var timespan = TimeSpan.FromMinutes(expirationMinutes ?? _options.DefaultCacheMinutes);
            if (_options.ProviderType == CachingProviderType.Hybrid)
                _hybridCachingProvider.SetAll(value, timespan);
            _easyCachingProvider.SetAll(value, timespan);
        }

        /// <summary>
        /// Sets all async.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="value">Dictionary of key/value cache items</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        public Task SetAllAsync<T>(IDictionary<string, T> value, int? expirationMinutes = null, CancellationToken cancellationToken = default)
        {
            var timespan = TimeSpan.FromMinutes(expirationMinutes ?? _options.DefaultCacheMinutes);
            if (_options.ProviderType == CachingProviderType.Hybrid)
                return _hybridCachingProvider.SetAllAsync(value, timespan);
            return _easyCachingProvider.SetAllAsync(value, timespan);
        }

        /// <summary>
        /// Sets the specified cacheKey, cacheValue and expiration async.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="cacheValue">Cache value</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        public Task SetAsync<T>(string cacheKey, T cacheValue, int? expirationMinutes = null, CancellationToken cancellationToken = default)
        {
            var timespan = TimeSpan.FromMinutes(expirationMinutes ?? _options.DefaultCacheMinutes);
            if (_options.ProviderType == CachingProviderType.Hybrid)
                return _hybridCachingProvider.SetAsync(cacheKey, cacheValue, timespan);
            return _easyCachingProvider.SetAsync(cacheKey, cacheValue, timespan);
        }

        /// <summary>
        /// Tries the set.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="cacheValue">Cache value</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        /// <returns>Determines whether it is successful</returns>
        public bool TrySet<T>(string cacheKey, T cacheValue, int? expirationMinutes = null)
        {
            var timespan = TimeSpan.FromMinutes(expirationMinutes ?? _options.DefaultCacheMinutes);
            if (_options.ProviderType == CachingProviderType.Hybrid)
                return _hybridCachingProvider.TrySet(cacheKey, cacheValue, timespan);
            return _easyCachingProvider.TrySet(cacheKey, cacheValue, timespan);
        }

        /// <summary>
        /// Tries the set async.
        /// </summary>
        /// <typeparam name="T">Type of cache value</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="cacheValue">Cache value</param>
        /// <param name="expirationMinutes">Expiration in minutes</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Determines whether it is successful</returns>
        public Task<bool> TrySetAsync<T>(string cacheKey, T cacheValue, int? expirationMinutes = null, CancellationToken cancellationToken = default)
        {
            var timespan = TimeSpan.FromMinutes(expirationMinutes ?? _options.DefaultCacheMinutes);
            if (_options.ProviderType == CachingProviderType.Hybrid)
                return _hybridCachingProvider.TrySetAsync(cacheKey, cacheValue, timespan);
            return _easyCachingProvider.TrySetAsync(cacheKey, cacheValue, timespan);
        }

        /// <summary>
        /// Increments the number stored at key by value. 
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="value">Increment value</param>
        /// <returns>Increased value</returns>
        public long IncrBy(string cacheKey, long value = 1)
        {
            switch (_options.ProviderType)
            {
                case CachingProviderType.InMemory:
                    var cacheValue = _easyCachingProvider.Get<long>(cacheKey).Value + value;
                    var expiration = _easyCachingProvider.GetExpiration(cacheKey);
                    _easyCachingProvider.Set(cacheKey, cacheValue, expiration);
                    return cacheValue;
                case CachingProviderType.Redis:
                    return _redisCachingProvider.IncrBy(cacheKey, value);
                default:
                    throw new NotSupportedException($"{nameof(IHybridCachingProvider)} dose not support {nameof(IncrBy)} method");
            }
        }

        /// <summary>
        /// Increments the number stored at key by value async. 
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="value">Increment value</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Increased value</returns>
        public async Task<long> IncrByAsync(string cacheKey, long value = 1, CancellationToken cancellationToken = default)
        {

            switch (_options.ProviderType)
            {
                case CachingProviderType.InMemory:
                    var cacheValue = _easyCachingProvider.Get<long>(cacheKey).Value + value;
                    var expiration = _easyCachingProvider.GetExpiration(cacheKey);
                    _easyCachingProvider.Set(cacheKey, cacheValue, expiration);
                    return cacheValue;
                case CachingProviderType.Redis:
                    return await _redisCachingProvider.IncrByAsync(cacheKey, value);
                default:
                    throw new NotSupportedException($"{nameof(IHybridCachingProvider)} dose not support {nameof(IncrByAsync)} method");
            }
        }

        /// <summary>
        /// Flush All Cached Item
        /// </summary>
        public void Flush()
        {
            if (_options.ProviderType == CachingProviderType.Hybrid)
                throw new NotSupportedException($"{nameof(IHybridCachingProvider)} dose not support {nameof(Flush)} method");
            _easyCachingProvider.Flush();
        }

        /// <summary>
        /// Flush All Cached Item async.
        /// </summary>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            if (_options.ProviderType == CachingProviderType.Hybrid)
                throw new NotSupportedException($"{nameof(IHybridCachingProvider)} dose not support {nameof(FlushAsync)} method");
            return _easyCachingProvider.FlushAsync();
        }

        #region GetCountFast
        private static readonly FieldInfo _serversField = typeof(DefaultRedisCachingProvider).GetField("_servers", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _cacheField = typeof(DefaultRedisCachingProvider).GetField("_cache", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo _handlePrefix = typeof(DefaultRedisCachingProvider).GetMethod("HandlePrefix", BindingFlags.Instance | BindingFlags.NonPublic);

        private int GetCountFast(string prefix = "")
        {
            var servers = (IEnumerable<IServer>)_serversField.GetValue(_redisCachingProvider);
            prefix = (string)_handlePrefix.Invoke(_redisCachingProvider, new object[] { prefix });

            var allCount = 0;
            if (string.IsNullOrWhiteSpace(prefix))
            {
                var _cache = (IDatabase)_cacheField.GetValue(_redisCachingProvider);

                foreach (var server in servers)
                    allCount += (int)server.DatabaseSize(_cache.Database);
            }
            else
            {
                foreach (var server in servers)
                {
                    ////KYES command
                    //var result = await server.ExecuteAsync("keys", prefix);
                    //var count = ((RedisValue[])result).Length;
                    ////SCAN command
                    //var result = server.Execute("SCAN", 0, "MATCH", prefix, "COUNT", 2147483647);
                    //var count = ((RedisValue[])((RedisResult[])result)[1]).Length;
                    ////Lua scripting with SCAN
                    //var result = server.Execute("eval", $"return #redis.call('SCAN', 0, 'MATCH', '{prefix}', 'COUNT', 2147483647)", 0);

                    //Lua scripting with KEYS
                    var result = server.Execute("eval", $"return #redis.call('KEYS', '{prefix}')", 0);
                    allCount += (int)result;
                }
            }
            return allCount;
        }

        private async Task<int> GetCountFastAsync(string prefix = "")
        {
            var servers = (IEnumerable<IServer>)_serversField.GetValue(_redisCachingProvider);
            prefix = (string)_handlePrefix.Invoke(_redisCachingProvider, new object[] { prefix });

            var allCount = 0;
            if (string.IsNullOrWhiteSpace(prefix))
            {
                var _cache = (IDatabase)_cacheField.GetValue(_redisCachingProvider);

                foreach (var server in servers)
                    allCount += (int)await server.DatabaseSizeAsync(_cache.Database);
            }
            else
            {
                foreach (var server in servers)
                {
                    //Lua scripting with KEYS
                    var result = await server.ExecuteAsync("eval", $"return #redis.call('KEYS', '{prefix}')", 0);
                    allCount += (int)result;
                }
            }
            return allCount;
        }
        #endregion
    }
}
