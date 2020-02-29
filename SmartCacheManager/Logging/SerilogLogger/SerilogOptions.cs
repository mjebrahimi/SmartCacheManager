using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace SmartCacheManager.Logging.Serilog
{
    /// <summary>
    /// Options to configure Serilog
    /// </summary>
    public class SerilogOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether enable timing logging. Default is true
        /// </summary>
        public bool TimingLogEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether enable cache logging. Default is true
        /// </summary>
        public bool CacheLogEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether enable logging. Default is true
        /// </summary>
        public bool LoggingEnabled { get; set; } = true;

        /// <summary>
        /// Disable file and console sinks when evnironment is not development. Default false
        /// </summary>
        public bool IsEnvDevelopment { get; set; } = false;

        #region Enrichers
        /// <summary>
        /// Enable CorrelationId enricher. Default is true
        /// </summary>
        public bool WithCorrelationId { get; set; } = true;

        /// <summary>
        /// Enable MemoryUsage enricher. Default is true
        /// </summary>
        public bool WithMemoryUsage { get; set; } = true;

        /// <summary>
        /// Enable ExceptionStackTraceHash enricher. Default is true
        /// </summary>
        public bool WithExceptionStackTraceHash { get; set; } = true;

        /// <summary>
        /// Enable ThreadId enricher. Default is true
        /// </summary>
        public bool WithThreadId { get; set; } = true;

        #endregion

        #region Console Sink
        /// <summary>
        /// Enable Console sink. Default is false
        /// </summary>
        public bool EnableConsole { get; set; } = false;

        /// <summary>
        /// The minimum level for events passed through the sink. Default is LogEventLevel.Information
        /// </summary>
        public LogEventLevel ConsoleMinimumLevel { get; set; } = LogEventLevel.Information;
        #endregion

        #region Debug Sink
        /// <summary>
        /// Enable Debug sink. Default is false
        /// </summary>
        public bool EnableDebug { get; set; } = false;

        /// <summary>
        /// The minimum level for events passed through the sink. Default is LogEventLevel.Information
        /// </summary>
        public LogEventLevel DebugMinimumLevel { get; set; } = LogEventLevel.Information;
        #endregion

        #region File Sink
        /// <summary>
        /// Enable File sink
        /// </summary>
        public bool EnableFile { get; set; } = false;

        /// <summary>
        /// Directory for log files. Default is 'C:\Serilog'
        /// </summary>
        public string FileDirectory { get; set; } = @"C:\\Serilog";

        /// <summary>
        /// The minimum level for events passed through the sink. Default is LogEventLevel.Warning
        /// </summary>
        public LogEventLevel FileMinimumLevel { get; set; } = LogEventLevel.Warning;

        /// <summary>
        /// The interval at which logging will roll over to a new file. (Example : log20190702.txt)
        /// </summary>
        public RollingInterval FileRollingInterval { get; set; } = RollingInterval.Day;

        /// <summary>
        /// The approximate maximum size, in bytes, to which a log file will be allowed to grow. Default is 300MB
        /// </summary>
        public long FileSizeLimitBytes { get; set; } = 314572800; //Default : 300MB
        #endregion

        #region EventLog Sink
        /// <summary>
        /// Enable EventLog (windows event viewer) sink
        /// </summary>
        public bool EnableEventLog { get; set; } = false;

        /// <summary>
        /// The source name by which the application is registered on the local computer. Default is 'SmartCacheManager'
        /// </summary>
        public string EventLogApplicationName { get; set; } = "SmartCacheManager"; // Assembly.GetExecutingAssembly().GetName().Name;

        /// <summary>
        /// The minimum level for events passed through the sink. Default is LogEventLevel.Warning
        /// </summary>
        public LogEventLevel EventLogMinimumLevel { get; set; } = LogEventLevel.Warning;
        #endregion

        #region Sql Sink
        /// <summary>
        /// Enable MSSQLServer sink. Default is true
        /// </summary>
        public bool EnableSqlLog { get; set; } = true;

        /// <summary>
        /// The connection string to the database where to store the events
        /// </summary>
        public string SqlConnectionString { get; set; }

        /// <summary>
        /// Name of the table to store the events in. Default is 'Logs'
        /// </summary>
        public string SqlTableName { get; set; } = "Logs";

        /// <summary>
        /// The minimum level for events passed through the sink. Default is LogEventLevel.Warning
        /// </summary>
        public LogEventLevel SqlMinimumLevel { get; set; } = LogEventLevel.Information;

        /// <summary>
        /// Default group of columns in database
        /// </summary>
        public ColumnOptions SqlColumns
        {
            get
            {
                if (_sqlColumns == null)
                {
                    var columnOptions = new ColumnOptions();
                    columnOptions.Store.Remove(StandardColumn.Properties);
                    columnOptions.Store.Add(StandardColumn.LogEvent);
                    columnOptions.LogEvent.ExcludeAdditionalProperties = true;
                    columnOptions.LogEvent.ExcludeStandardColumns = true;
                    //columnOptions.TimeStamp.ConvertToUtc = true;
                    _sqlColumns = columnOptions;
                }
                return _sqlColumns;
            }
            set
            {
                _sqlColumns = value;
            }
        }
        private ColumnOptions _sqlColumns;
        #endregion
    }
}
