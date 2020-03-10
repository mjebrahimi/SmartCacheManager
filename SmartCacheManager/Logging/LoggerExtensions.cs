using SmartCacheManager.Utilities;
using System;

namespace SmartCacheManager.Logging
{
    public static class LoggerExtensions
    {
        #region Write per Level
        /// <summary>
        /// Write a log event with the Trace level.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the messageTemplate template.</param>
        public static void LogTrace(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            logger.Log(LogLevel.Trace, null, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the Trace level and associated exception.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the messageTemplate template.</param>
        public static void LogTrace(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            logger.Log(LogLevel.Trace, exception, messageTemplate, propertyValues);
        }
        /// <summary>
        /// Write a log event with the Debug level.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the messageTemplate template.</param>
        public static void LogDebug(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            logger.Log(LogLevel.Debug, null, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the Debug level and associated exception.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the messageTemplate template.</param>
        public static void LogDebug(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            logger.Log(LogLevel.Debug, exception, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the Information level.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the messageTemplate template.</param>
        public static void LogInformation(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            logger.Log(LogLevel.Information, null, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the Information level and associated exception.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the messageTemplate template.</param>
        public static void LogInformation(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            logger.Log(LogLevel.Information, exception, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the Warning level.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the messageTemplate template.</param>
        public static void LogWarning(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            logger.Log(LogLevel.Warning, null, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the Warning level and associated exception.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the messageTemplate template.</param>
        public static void LogWarning(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            logger.Log(LogLevel.Warning, exception, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the Error level.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the messageTemplate template.</param>
        public static void LogError(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            logger.Log(LogLevel.Error, null, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the Error level and associated exception.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the messageTemplate template.</param>
        public static void LogError(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            logger.Log(LogLevel.Error, exception, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the Critical level.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the messageTemplate template.</param>
        public static void LogCritical(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            logger.Log(LogLevel.Critical, null, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the Critical level and associated exception.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the messageTemplate template.</param>
        public static void LogCritical(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            logger.Log(LogLevel.Critical, exception, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write an event to the log.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="logLevel">The level of the log.</param>
        /// <param name="exception">Exception related to the log.</param>
        /// <param name="messageTemplate">Message template describing the log.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        public static void Log(this ILogger logger, LogLevel logLevel, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            messageTemplate.NotNull(nameof(messageTemplate));

            if (logLevel == LogLevel.None) return;

            // Catch a common pitfall when a single non-object array is cast to object[]
            if (propertyValues != null &&
                propertyValues.GetType() != typeof(object[]))
                propertyValues = new object[] { propertyValues };

            logger.Log(logLevel, exception, messageTemplate, propertyValues, null);
        }
        #endregion

        #region BeginLogScope
        /// <summary>
        /// Create and begin a log scope with Trace log level to collecting properties to log when disposed
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="messageTemplate">Message template describing the event</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <returns>An IDisposable that ends the log operation scope on dispose.</returns>
        public static IDisposable BeginLogScopeTrace(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            return logger.BeginLogScope(LogLevel.Trace, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Create and begin a log scope with Debug log level to collecting properties to log when disposed
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="messageTemplate">Message template describing the event</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <returns>An IDisposable that ends the log operation scope on dispose.</returns>
        public static IDisposable BeginLogScopeDebug(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            return logger.BeginLogScope(LogLevel.Debug, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Create and begin a log scope with Information log level to collecting properties to log when disposed
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="messageTemplate">Message template describing the event</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <returns>An IDisposable that ends the log operation scope on dispose.</returns>
        public static IDisposable BeginLogScopeInformation(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            return logger.BeginLogScope(LogLevel.Information, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Create and begin a log scope with Warning log level to collecting properties to log when disposed
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="messageTemplate">Message template describing the event</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <returns>An IDisposable that ends the log operation scope on dispose.</returns>
        public static IDisposable BeginLogScopeWarning(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            return logger.BeginLogScope(LogLevel.Warning, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Create and begin a log scope with Error log level to collecting properties to log when disposed
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="messageTemplate">Message template describing the event</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <returns>An IDisposable that ends the log operation scope on dispose.</returns>
        public static IDisposable BeginLogScopeError(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            return logger.BeginLogScope(LogLevel.Error, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Create and begin a log scope with Critical log level to collecting properties to log when disposed
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="messageTemplate">Message template describing the event</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <returns>An IDisposable that ends the log operation scope on dispose.</returns>
        public static IDisposable BeginLogScopeCritical(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            return logger.BeginLogScope(LogLevel.Critical, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Create and begin a log scope to collecting properties to log when disposed
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="logLevel">The level of the log.</param>
        /// <param name="messageTemplate">Message template describing the event</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <returns>An IDisposable that ends the log operation scope on dispose.</returns>
        public static IDisposable BeginLogScope(this ILogger logger, LogLevel logLevel, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            messageTemplate.NotNull(nameof(messageTemplate));

            if (logLevel == LogLevel.None) return new NullDisposable();

            // Catch a common pitfall when a single non-object array is cast to object[]
            if (propertyValues != null &&
                propertyValues.GetType() != typeof(object[]))
                propertyValues = new object[] { propertyValues };

            return logger.BeginLogScope(logLevel, messageTemplate, propertyValues, null);
        }
        #endregion

        /// <summary>
        /// Push a property onto the context, returning an IDisposable that must later be used to remove the property,
        /// along with any others that may have been pushed on top of it and not yet popped. 
        /// The property must be popped from the same thread/logical call context.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyValue">The value of the property.</param>
        /// <param name="destructureObjects">
        /// Determines whether it must be destruct.
        /// If true, and the value is a non-primitive, non-array type, then the value will be converted to a structure; otherwise, unknown types will be converted to scalars, which are generally stored as strings.
        /// </param>
        /// <returns>A handle to later remove the property from the context.</returns>
        public static IDisposable BeginScope(this ILogger logger, string propertyName, object propertyValue, bool destructureObjects = false)
        {
            logger.NotNull(nameof(logger));
            return logger.BeginScope(new LogProperty(propertyName, propertyValue, destructureObjects));
        }

        /// <summary>
        /// Add a property to the log scope if not already present, otherwise, update its value.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyValue">The value of the property.</param>
        /// <param name="destructureObjects">
        /// Determines whether it must be destruct.
        /// If true, and the value is a non-primitive, non-array type, then the value will be converted to a structure; otherwise, unknown types will be converted to scalars, which are generally stored as strings.
        /// </param>
        public static void SetProperty(this ILogger logger, string propertyName, object propertyValue, bool destructureObjects = false)
        {
            logger.NotNull(nameof(logger));
            logger.SetProperty(new LogProperty(propertyName, propertyValue, destructureObjects));
        }

        /// <summary>
        /// Log error if was not logged before
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the messageTemplate template.</param>
        /// <returns>Return false always to not catch exception in when condation</returns>
        public static bool LogErrorIfNotBefore(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.NotNull(nameof(logger));
            exception.NotNull(nameof(exception));
            messageTemplate.NotNull(nameof(messageTemplate));

            if (LogConstants.LogErrorEnabled && !exception.IsLoggedBefore())
            {
                logger.SetProperty(LogConstants.Exception, exception);
                logger.SetProperty(LogConstants.Level, LogLevel.Error);
                logger.Log(LogLevel.Error, exception, messageTemplate, propertyValues, new[] { new LogProperty(LogConstants.LogError, null) });
                exception.MarkAsLogged();
            }
            return false;
        }


        #region Experimental Code
        //private static (List<object>, List<LogProperty>) GetProperties(params object[] properties)
        //{
        //    if (properties == null)
        //        return (null, null);
        //
        //    var propertyValues = new List<object>();
        //    var logProperties = new List<LogProperty>();
        //
        //    foreach (var property in properties)
        //    {
        //        switch (property)
        //        {
        //            case IEnumerable enumerable when !(enumerable is string):
        //                foreach (var item in enumerable)
        //                    SetProperty(item);
        //                break;
        //            default:
        //                SetProperty(property);
        //                break;
        //        }
        //    }
        //    return (propertyValues, logProperties);
        //
        //    void SetProperty(object property)
        //    {
        //        property.NotNull(nameof(property));
        //
        //        if (property is LogProperty propertyValue)
        //        {
        //            logProperties.Add(propertyValue);
        //            return;
        //        }
        //
        //        var type = property.GetType();
        //        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
        //        {
        //            dynamic item = property;
        //            logProperties.Add(new LogProperty(item.Key, item.Value));
        //            return;
        //        }
        //
        //        propertyValues.Add(property);
        //    }
        //}
        #endregion
    }
}
