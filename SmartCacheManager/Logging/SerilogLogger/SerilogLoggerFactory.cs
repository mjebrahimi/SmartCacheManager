using SmartCacheManager.Utilities;
using Serilog.Extensions.Hosting;
using System;

namespace SmartCacheManager.Logging.Serilog
{
    /// <summary>
    /// Serilog implementation of ILoggerFactory
    /// </summary>
    public class SerilogLoggerFactory : ILoggerFactory
    {
        private readonly global::Serilog.ILogger _logger;
        private readonly DiagnosticContext _diagnosticContext;

        public SerilogLoggerFactory(global::Serilog.ILogger logger, DiagnosticContext diagnosticContext)
        {
            _diagnosticContext = diagnosticContext.NotNull(nameof(diagnosticContext));
            _logger = logger.NotNull(nameof(logger));
        }

        /// <summary>
        /// Create a logger that marks logs as being from the specified source type.
        /// </summary>
        /// <param name="source">Type of source context</param>
        /// <returns>ILogger</returns>
        public ILogger CreateLogger(Type source)
        {
            var logger = _logger.ForContext(source);
            return new SerilogLogger(logger, _diagnosticContext);
        }

        /// <summary>
        /// Create a logger that marks logs as being from the specified source type.
        /// </summary>
        /// <typeparam name="TSource">Type of source context</typeparam>
        /// <returns>ILogger</returns>
        public ILogger<TSource> CreateLogger<TSource>()
        {
            return new SerilogLogger<TSource>(_logger, _diagnosticContext);
        }
    }
}
