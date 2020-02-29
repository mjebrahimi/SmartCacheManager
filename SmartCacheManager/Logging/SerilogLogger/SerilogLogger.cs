using SmartCacheManager.Utilities;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartCacheManager.Logging.Serilog
{
    /// <summary>
    /// Serilog implementation of ILogger with soruce context
    /// </summary>
    public class SerilogLogger<TSource> : SerilogLogger, ILogger<TSource>
    {
        public SerilogLogger(global::Serilog.ILogger logger, DiagnosticContext diagnosticContext)
            : base(logger.ForContext<TSource>(), diagnosticContext)
        {
        }
    }

    /// <summary>
    /// Serilog implementation of ILogger
    /// </summary>
    public class SerilogLogger : ILogger
    {
        private readonly global::Serilog.ILogger _logger;
        private readonly DiagnosticContext _diagnosticContext;
        private static readonly LogEventProperty[] NoProperties = Array.Empty<LogEventProperty>();

        public SerilogLogger(global::Serilog.ILogger logger, DiagnosticContext diagnosticContext)
        {
            _diagnosticContext = diagnosticContext.NotNull(nameof(diagnosticContext));
            _logger = logger.NotNull(nameof(logger));
        }

        /// <summary>
        /// Write an event to the log.
        /// </summary>
        /// <param name="logLevel">The level of the log.</param>
        /// <param name="exception">Exception related to the log.</param>
        /// <param name="messageTemplate">Message template describing the log.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <param name="logProperties">Properties associated with the log.</param>
        [MessageTemplateFormatMethod("messageTemplate")]
        public void Log(LogLevel logLevel, Exception exception, string messageTemplate, IEnumerable<object> propertyValues, IEnumerable<LogProperty> logProperties)
        {
            messageTemplate.NotNull(nameof(messageTemplate));

            if (logLevel == LogLevel.None) return;
            var level = ToSerilogLevel(logLevel);

            _logger.BindMessageTemplate(messageTemplate, GetPropertyArray(propertyValues), out var parsedTemplate, out var boundProperties);

            var logEventProperties = logProperties?.Select(p =>
            {
                if (_logger.BindProperty(p.Name, p.Value, p.DestructureObjects, out var property) == false)
                    throw new InvalidOperationException($"Can not bind property to serilog logger. Name : {p.Name} - Value : {p.Value} - DestructureObjects : {p.DestructureObjects}");
                return property;
            }) ?? NoProperties;

            var properties = boundProperties.Concat(logEventProperties);

            var logEvent = new LogEvent(DateTimeOffset.Now, level, exception, parsedTemplate, properties);
            _logger.Write(logEvent);
        }

        /// <summary>
        /// Create and begin a log scope to collecting properties to log, returning an IDisposable that must later be used to write log.
        /// </summary>
        /// <param name="logLevel">The level of the log.</param>
        /// <param name="messageTemplate">Message template describing the event</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <param name="logProperties">Properties associated with the log.</param>
        /// <returns>An IDisposable that ends the log operation scope on dispose.</returns>
        [MessageTemplateFormatMethod("messageTemplate")]
        public IDisposable BeginLogScope(LogLevel logLevel, string messageTemplate, IEnumerable<object> propertyValues, IEnumerable<LogProperty> logProperties)
        {
            messageTemplate.NotNull(nameof(messageTemplate));

            if (logLevel == LogLevel.None) return new NullDisposable();
            var level = ToSerilogLevel(logLevel);

            return new LogScope(_logger, _diagnosticContext, level, messageTemplate, GetPropertyArray(propertyValues), logProperties);
        }

        /// <summary>
        /// Push a property onto the context, returning an IDisposable that must later be used to remove the property,
        /// along with any others that may have been pushed on top of it and not yet popped. 
        /// The property must be popped from the same thread/logical call context.
        /// </summary>
        /// <param name="logProperty">Property associated with the log</param>
        /// <returns>A handle to later remove the property from the context.</returns>
        public IDisposable BeginScope(LogProperty logProperty)
        {
            logProperty.NotNull(nameof(logProperty));
            if (LogEventProperty.IsValidName(logProperty.Name) == false)
                throw new ArgumentException($"The name of property is not valid '{logProperty.Name}'");

            return LogContext.PushProperty(logProperty.Name, logProperty.Value, logProperty.DestructureObjects);
        }

        /// <summary>
        /// Remove all properties from LogContext for the current async scope.
        /// </summary>
        public void ResetScope()
        {
            LogContext.Reset();
        }

        /// <summary>
        /// Remove all properties from the LogContext, returning an IDisposable that must later be used to restore enrichers that were on the stack before Suspend() was called.
        /// </summary>
        /// <returns>A handle that must be disposed, in order, to restore properties back to the stack.</returns>
        public IDisposable SuspendScope()
        {
            return LogContext.Suspend();
        }

        /// <summary>
        /// Add a property to the log scope if not already present, otherwise, update its value.
        /// </summary>
        /// <param name="logProperty">Property associated with the log</param>
        public void SetProperty(LogProperty logProperty)
        {
            logProperty.NotNull(nameof(logProperty));
            if (LogEventProperty.IsValidName(logProperty.Name) == false)
                throw new ArgumentException($"The name of property is not valid '{logProperty.Name}'");

            _diagnosticContext.Set(logProperty.Name, logProperty.Value, logProperty.DestructureObjects);
        }

        private object[] GetPropertyArray(IEnumerable<object> propertyValues)
        {
            return propertyValues == null ? null :
                (propertyValues is object[] array ? array : propertyValues.ToArray());
        }

        /// <summary>
        /// Convert <paramref name="logLevel"/> to the equivalent Serilog <see cref="LogEventLevel"/>.
        /// </summary>
        /// <param name="logLevel">A SmartCacheManager.Logging <see cref="LogLevel"/>.</param>
        /// <returns>The Serilog equivalent of <paramref name="logLevel"/>.</returns>
        private LogEventLevel ToSerilogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return LogEventLevel.Verbose;
                case LogLevel.Debug:
                    return LogEventLevel.Debug;
                case LogLevel.Information:
                    return LogEventLevel.Information;
                case LogLevel.Warning:
                    return LogEventLevel.Warning;
                case LogLevel.Error:
                    return LogEventLevel.Error;
                case LogLevel.Critical:
                    return LogEventLevel.Fatal;
                case LogLevel.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        #region LogScope class
        /// <summary>
        /// An log scope to collecting properties to log when disposed
        /// </summary>
        class LogScope : IDisposable
        {
            private readonly global::Serilog.ILogger _logger;
            private readonly DiagnosticContextCollector _collector;
            private readonly LogEventLevel _level;
            private readonly string _messageTemplate;
            private readonly object[] _propertyValues;
            private readonly IEnumerable<LogProperty> _logProperties;

            public LogScope(global::Serilog.ILogger logger, DiagnosticContext diagnosticContext, LogEventLevel level, string messageTemplate,
                object[] propertyValues, IEnumerable<LogProperty> logProperties)
            {
                _logger = logger.NotNull(nameof(logger));
                _collector = diagnosticContext.BeginCollection();
                _level = level;
                _messageTemplate = messageTemplate.NotNull(nameof(messageTemplate));
                _propertyValues = propertyValues;
                _logProperties = logProperties;
            }

            public void Dispose()
            {
                try
                {
                    if (!_collector.TryComplete(out var collectedProperties))
                        collectedProperties = NoProperties;

                    _logger.BindMessageTemplate(_messageTemplate, _propertyValues, out var parsedTemplate, out var boundProperties);

                    var logEventProperties = _logProperties?.Select(p =>
                    {
                        if (_logger.BindProperty(p.Name, p.Value, p.DestructureObjects, out var property) == false)
                            throw new InvalidOperationException($"Can not bind property to serilog logger. Name : {p.Name} - Value : {p.Value} - DestructureObjects : {p.DestructureObjects}");
                        return property;
                    }) ?? NoProperties;

                    var properties = boundProperties.Concat(logEventProperties).Concat(collectedProperties);

                    var logEvent = new LogEvent(DateTimeOffset.Now, _level, null, parsedTemplate, properties);
                    _logger.Write(logEvent);
                }
                finally
                {
                    _collector.Dispose();
                }
            }
        }
        #endregion
    }
}
