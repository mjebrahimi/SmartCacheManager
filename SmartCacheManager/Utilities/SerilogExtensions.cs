using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Filters;
using System.Linq;

namespace SmartCacheManager.Utilities
{
    public static class SerilogExtensions
    {
        /// <summary>
        /// Filter log events to include only those that has EventId with specified id
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <param name="id">The id of EventId</param>
        /// <returns>LoggerConfiguration</returns>
        public static LoggerConfiguration FilterByEventId(this LoggerFilterConfiguration configuration, int id)
        {
            var scalarValue = new ScalarValue(id);
            return configuration.ByIncludingOnly(logEvent =>
            {
                if (logEvent.Properties.TryGetValue(nameof(EventId), out var propertyValue) && propertyValue is StructureValue structureValue)
                {
                    var idValue = structureValue.Properties.Where(cc => cc.Name == nameof(EventId.Id)).FirstOrDefault();
                    return scalarValue.Equals(idValue.Value);
                }
                return false;
            });
        }

        /// <summary>
        /// Filter log events to include only those that has specified property
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <param name="propertyName">Name of property</param>
        /// <param name="propertyValue">Expected property value</param>
        /// <returns>LoggerConfiguration</returns>
        public static LoggerConfiguration FilterByProperty(this LoggerFilterConfiguration configuration, string propertyName, string propertyValue)
        {
            return configuration.ByIncludingOnly(Matching.WithProperty(propertyName, propertyValue)); //new ScalarValue(propertyValue)

            //var scalarValue = new ScalarValue(propertyValue);
            //return configuration.ByIncludingOnly(logEvent =>
            //{
            //    if (logEvent.Properties.TryGetValue(propertyName, out var prop) && prop is ScalarValue value)
            //        return scalarValue.Equals(value);
            //    return false;
            //});
        }

        /// <summary>
        /// Filter log events to include only those that has specified property
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <param name="propertyName">Name of property</param>
        /// <param name="propertyValue">Expected property value</param>
        /// <returns>LoggerConfiguration</returns>
        public static LoggerConfiguration FilterByHasProperty(this LoggerFilterConfiguration configuration, string propertyName)
        {
            return configuration.ByIncludingOnly(Matching.WithProperty(propertyName));

            //return configuration.ByIncludingOnly(logEvent => logEvent.Properties.ContainsKey(propertyName));
        }
    }
}
