using SmartCacheManager.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Extensions.Hosting;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;

namespace SmartCacheManager.Logging.Serilog
{
    public static class SerilogConfigurationExtensions
    {
        /// <summary>
        /// Add SerilogLogger to services
        /// </summary>
        /// <param name="services">services</param>
        /// <param name="configure">Action to configure SerilogOptions (Enrichers, Sinks and ...)</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddSerilogLogger(this IServiceCollection services, Action<SerilogOptions> configure = null)
        {
            services.NotNull(nameof(services));

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                Log.CloseAndFlush();
            };

            //TODO: services.Configure()
            var options = new SerilogOptions();
            configure?.Invoke(options);

            var logger = options.LoggingEnabled ? CreateLogger() : global::Serilog.Core.Logger.None;
            var diagnosticContext = new DiagnosticContext(logger);
            var serilogLogger = new SerilogLogger(logger, diagnosticContext);
            var serilogLoggerFactory = new SerilogLoggerFactory(logger, diagnosticContext);

            LoggerExtensions.LoggingEnabled = options.LoggingEnabled;
            MethodTimeLogger.Enabled = options.LoggingEnabled && options.TimingLogEnabled;
            MethodTimeLogger.Logger = serilogLogger;

            services.TryAddSingleton<ILogger>(serilogLogger);
            services.TryAddSingleton<ILoggerFactory>(serilogLoggerFactory);

            return services;

            global::Serilog.ILogger CreateLogger()
            {
                var config = new LoggerConfiguration();

                #region Enrichers
                config.Enrich.FromLogContext();

                //https://www.nuget.org/packages/Serilog.Exceptions/
                //config.Enrich.WithExceptionDetails();

                if (options.WithCorrelationId)
                {
                    services.AddHttpContextAccessor();
                    config.Enrich.WithCorrelationId();
                }

                if (options.WithMemoryUsage)
                    config.Enrich.WithMemoryUsage();

                if (options.WithExceptionStackTraceHash)
                    config.Enrich.WithExceptionStackTraceHash();

                if (options.WithThreadId)
                    config.Enrich.WithThreadId();
                #endregion

                #region Sinks
                if (options.IsEnvDevelopment)
                {
                    if (options.EnableConsole)
                        config.WriteTo.Console(options.ConsoleMinimumLevel, theme: AnsiConsoleTheme.Code);

                    if (options.EnableDebug)
                        config.WriteTo.Debug(options.DebugMinimumLevel);
                }

                if (options.EnableFile)
                {
                    var filePath = Path.Combine(options.FileDirectory, "SmartCacheManager_Log_{Date}.txt");
                    config.WriteTo.File(filePath, restrictedToMinimumLevel: options.FileMinimumLevel,
                        fileSizeLimitBytes: options.FileSizeLimitBytes, rollingInterval: options.FileRollingInterval, rollOnFileSizeLimit: true);
                }

                if (options.EnableEventLog)
                {
                    config.WriteTo.EventLog(options.EventLogApplicationName,
                    restrictedToMinimumLevel: options.EventLogMinimumLevel,
                    manageEventSource: true);
                }

                if (options.EnableSqlLog)
                {
                    options.SqlConnectionString.NotNullOrWhiteSpace(nameof(options.SqlConnectionString));

                    SqlHelper.CreateDatabaseIfNotExists(options.SqlConnectionString);

                    config.LogErrorToSqlServer(options.SqlConnectionString, LogEventLevel.Error);

                    if (options.TimingLogEnabled)
                        config.LogTimingToSqlServer(options.SqlConnectionString, LogEventLevel.Information);

                    if (options.CacheLogEnabled)
                        config.LogCacheToSqlServer(options.SqlConnectionString, LogEventLevel.Information);
                }
                #endregion

                return config.CreateLogger();
            }
        }

        /// <summary>
        /// Add custom SerilogLogger to services
        /// </summary>
        /// <param name="services">services</param>
        /// <param name="loggerConfigure">Action to configure serilog LoggerConfiguration</param>
        /// <returns>IServiceCollection</returns>
        /// <summary>
        public static IServiceCollection AddCustomSerilogLogger(this IServiceCollection services, Action<LoggerConfiguration> loggerConfigure = null)
        {
            services.NotNull(nameof(services));

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                Log.CloseAndFlush();
            };

            var config = new LoggerConfiguration();

            //Customize LoggerConfiguration
            loggerConfigure?.Invoke(config);

            var logger = config.CreateLogger();
            //var logger = config.CreateLogger();
            var diagnosticContext = new DiagnosticContext(logger);
            var serilogLogger = new SerilogLogger(logger, diagnosticContext);
            var serilogLoggerFactory = new SerilogLoggerFactory(logger, diagnosticContext);

            MethodTimeLogger.Logger = serilogLogger;

            services.TryAddSingleton<ILogger>(serilogLogger);
            services.TryAddSingleton<ILoggerFactory>(serilogLoggerFactory);

            return services;
        }

        #region Filters
        /// <summary>
        /// Filter log events to include only error logs
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <returns>LoggerConfiguration</returns>
        public static LoggerConfiguration FilterOnlyErrorLogs(this LoggerFilterConfiguration configuration)
        {
            return configuration.ByIncludingOnly(logEvent =>
            {
                var propertyName = LogConstants.LogError;
                var contains = logEvent.Properties.ContainsKey(propertyName);
                logEvent.RemovePropertyIfPresent(propertyName);
                return contains;
            });
        }

        /// <summary>
        /// Filter log events to include only timing logs
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <returns>LoggerConfiguration</returns>
        public static LoggerConfiguration FilterOnlyTimingLogs(this LoggerFilterConfiguration configuration)
        {
            return configuration.ByIncludingOnly(logEvent =>
            {
                var propertyName = LogConstants.LogTiming;
                var contains = logEvent.Properties.ContainsKey(propertyName);
                logEvent.RemovePropertyIfPresent(propertyName);
                return contains;
            });
        }

        /// <summary>
        /// Filter log events to include only cache logs
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <returns>LoggerConfiguration</returns>
        public static LoggerConfiguration FilterOnlyCacheLogs(this LoggerFilterConfiguration configuration)
        {
            return configuration.ByIncludingOnly(logEvent =>
            {
                var propertyName = LogConstants.LogCache;
                var contains = logEvent.Properties.ContainsKey(propertyName);
                logEvent.RemovePropertyIfPresent(propertyName);
                return contains;
            });
        }
        #endregion

        #region WriteTo SqlServer
        public static void LogErrorToSqlServer(this LoggerConfiguration configuration, string connectionString, LogEventLevel minimumLevel)
        {
            var columnOptions = new ColumnOptions();
            columnOptions.Store.Remove(StandardColumn.Level);
            columnOptions.Store.Remove(StandardColumn.Properties);
            columnOptions.Store.Add(StandardColumn.LogEvent);

            columnOptions.LogEvent.ExcludeAdditionalProperties = true;
            columnOptions.LogEvent.ExcludeStandardColumns = true;
            //columnOptions.TimeStamp.ConvertToUtc = true;

            columnOptions.AdditionalColumns = new Collection<SqlColumn>
            {
                new SqlColumn {ColumnName = "MethodName", DataType = SqlDbType.VarChar, DataLength = 50, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "SourceContext", DataType = SqlDbType.VarChar, DataLength = 150, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "ExceptionStackTraceHash", DataType = SqlDbType.BigInt, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "CorrelationId", DataType = SqlDbType.VarChar, DataLength = 36, NonClusteredIndex = true},
            };

            configuration.WriteTo.Logger(subLogger =>
            {
                subLogger
                    .Filter.FilterOnlyErrorLogs()
                    .WriteTo.MSSqlServer(connectionString,
                        tableName: LogConstants.LogError,
                        columnOptions: columnOptions,
                        restrictedToMinimumLevel: minimumLevel,
                        autoCreateSqlTable: true);
            });
        }

        public static void LogTimingToSqlServer(this LoggerConfiguration configuration, string connectionString, LogEventLevel minimumLevel)
        {
            var columnOptions = new ColumnOptions();
            columnOptions.Store.Remove(StandardColumn.Level);
            columnOptions.Store.Remove(StandardColumn.Message);
            columnOptions.Store.Remove(StandardColumn.MessageTemplate);
            columnOptions.Store.Remove(StandardColumn.Exception);
            columnOptions.Store.Remove(StandardColumn.Properties);
            columnOptions.Store.Add(StandardColumn.LogEvent);

            columnOptions.LogEvent.ExcludeAdditionalProperties = true;
            columnOptions.LogEvent.ExcludeStandardColumns = true;
            //columnOptions.TimeStamp.ConvertToUtc = true;

            columnOptions.AdditionalColumns = new Collection<SqlColumn>
            {
                new SqlColumn {ColumnName = "MethodName", DataType = SqlDbType.VarChar, DataLength = 300, AllowNull = false},
                new SqlColumn {ColumnName = "ElapsedMilliSeconds", DataType = SqlDbType.BigInt, AllowNull = false, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "MethodHashCode", DataType = SqlDbType.BigInt, AllowNull = false, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "CorrelationId", DataType = SqlDbType.VarChar, DataLength = 36, NonClusteredIndex = true},
            };

            configuration.WriteTo.Logger(subLogger =>
            {
                subLogger
                    .Filter.FilterOnlyTimingLogs()
                    .WriteTo.MSSqlServer(connectionString,
                        tableName: LogConstants.LogTiming,
                        columnOptions: columnOptions,
                        restrictedToMinimumLevel: minimumLevel,
                        autoCreateSqlTable: true);
            });
        }

        public static void LogCacheToSqlServer(this LoggerConfiguration configuration, string connectionString, LogEventLevel minimumLevel)
        {
            var columnOptions = new ColumnOptions();
            columnOptions.Store.Remove(StandardColumn.Properties);
            columnOptions.Store.Remove(StandardColumn.Level);
            columnOptions.Store.Remove(StandardColumn.Exception);
            columnOptions.Store.Add(StandardColumn.LogEvent);

            columnOptions.LogEvent.ExcludeAdditionalProperties = true;
            columnOptions.LogEvent.ExcludeStandardColumns = true;
            //columnOptions.TimeStamp.ConvertToUtc = true;

            columnOptions.AdditionalColumns = new Collection<SqlColumn>
            {
                new SqlColumn {ColumnName = "Exception", DataType = SqlDbType.NVarChar, DataLength = -1},
                new SqlColumn {ColumnName = "SupplierType", DataType = SqlDbType.NVarChar, DataLength = 50, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "CurrentRPM", DataType = SqlDbType.Decimal, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "CacheMinutesBySearchDate", DataType = SqlDbType.Int, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "CacheMinutesByCurrentRPM", DataType = SqlDbType.Int, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "CurrentExpirationMinutes", DataType = SqlDbType.Decimal, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "SearchDate", DataType = SqlDbType.SmallDateTime, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "NowDateTime", DataType = SqlDbType.SmallDateTime, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "SearchDiffHours", DataType = SqlDbType.Decimal, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "IsLimitationReached", DataType = SqlDbType.Bit, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "IncomingRequestHashCode", DataType = SqlDbType.BigInt, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "OutgoingRequestHashCode", DataType = SqlDbType.BigInt, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "CorrelationId", DataType = SqlDbType.VarChar, DataLength = 36, NonClusteredIndex = true},
                new SqlColumn {ColumnName = "MemoryUsage", DataType = SqlDbType.BigInt, NonClusteredIndex = true},
            };

            configuration.WriteTo.Logger(subLogger =>
            {
                subLogger
                    .Filter.FilterOnlyCacheLogs()
                    .WriteTo.MSSqlServer(connectionString,
                        tableName: LogConstants.LogCache,
                        columnOptions: columnOptions,
                        restrictedToMinimumLevel: minimumLevel,
                        autoCreateSqlTable: true);
            });
        }
        #endregion

        #region Custom WriteTo
        public static LoggerConfiguration LogErrorWriteTo(this LoggerConfiguration configuration, Action<LoggerConfiguration> configureLogger)
        {
            configuration.WriteTo.Logger(config =>
            {
                config.Filter.FilterOnlyErrorLogs();
                configureLogger(config);
            });
            return configuration;
        }

        public static LoggerConfiguration LogTimingWriteTo(this LoggerConfiguration configuration, Action<LoggerConfiguration> configureLogger)
        {
            configuration.WriteTo.Logger(config =>
            {
                config.Filter.FilterOnlyTimingLogs();
                configureLogger(config);
            });
            return configuration;
        }

        public static LoggerConfiguration LogCacheWriteTo(this LoggerConfiguration configuration, Action<LoggerConfiguration> configureLogger)
        {
            configuration.WriteTo.Logger(config =>
            {
                config.Filter.FilterOnlyCacheLogs();
                configureLogger(config);
            });
            return configuration;
        }
        #endregion
    }
}
