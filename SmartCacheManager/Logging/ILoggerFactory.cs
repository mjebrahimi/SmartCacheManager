using System;

namespace SmartCacheManager.Logging
{
    /// <summary>
    /// Logger factory abstraction
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Create a logger that marks logs as being from the specified source type.
        /// </summary>
        /// <param name="source">Type of source context</param>
        /// <returns>ILogger</returns>
        ILogger CreateLogger(Type source);

        /// <summary>
        /// Create a logger that marks logs as being from the specified source type.
        /// </summary>
        /// <typeparam name="TSource">Type of source context</typeparam>
        /// <returns>ILogger</returns>
        ILogger<TSource> CreateLogger<TSource>();
    }
}