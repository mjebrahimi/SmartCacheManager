using System;
using System.Collections.Generic;

namespace SmartCacheManager.Logging
{
    /// <summary>
    /// Logger abstraction with soruce context
    /// </summary>
    /// <typeparam name="TSource">Type of source context</typeparam>
    public interface ILogger<out TSource> : ILogger
    {
    }

    /// <summary>
    /// Logger abstraction
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Write an event to the log.
        /// </summary>
        /// <param name="logLevel">The level of the log.</param>
        /// <param name="exception">Exception related to the log.</param>
        /// <param name="messageTemplate">Message template describing the log.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <param name="logProperties">Properties associated with the log.</param>
        void Log(LogLevel logLevel, Exception exception, string messageTemplate, IEnumerable<object> propertyValues, IEnumerable<LogProperty> logProperties);

        /// <summary>
        /// Create and begin a log scope to collecting properties to log, returning an IDisposable that must later be used to write log.
        /// </summary>
        /// <param name="logLevel">The level of the log.</param>
        /// <param name="messageTemplate">Message template describing the event</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <param name="logProperties">Properties associated with the log.</param>
        /// <returns>An IDisposable that ends the log operation scope on dispose.</returns>
        IDisposable BeginLogScope(LogLevel logLevel, string messageTemplate, IEnumerable<object> propertyValues, IEnumerable<LogProperty> logProperties);

        /// <summary>
        /// Add a property to the log scope if not already present, otherwise, update its value.
        /// </summary>
        /// <param name="logProperty">Property associated with the log</param>
        void SetProperty(LogProperty logProperty);

        /// <summary>
        /// Push a property onto the context, returning an IDisposable that must later be used to remove the property,
        /// along with any others that may have been pushed on top of it and not yet popped. 
        /// The property must be popped from the same thread/logical call context.
        /// </summary>
        /// <param name="logProperty">Property associated with the log</param>
        /// <returns>A handle to later remove the property from the context.</returns>
        IDisposable BeginScope(LogProperty logProperty);

        /// <summary>
        /// Remove all properties from LogContext for the current async scope.
        /// </summary>
        void ResetScope();

        /// <summary>
        /// Remove all properties from the LogContext, returning an IDisposable that must later be used to restore enrichers that were on the stack before Suspend() was called.
        /// </summary>
        /// <returns>A handle that must be disposed, in order, to restore properties back to the stack.</returns>
        IDisposable SuspendScope();
    }
}