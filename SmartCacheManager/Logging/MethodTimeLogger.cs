using System.Reflection;

namespace SmartCacheManager.Logging
{
    public static class MethodTimeLogger
    {
        public static ILogger Logger { get; set; }

        public static void Log(MethodBase methodBase, long milliseconds, string message)
        {
#if RELEASE
            if (!LogConstants.LogTimingEnabled) return;

            var typeName = methodBase.DeclaringType.Name;
            var methodName = $"{typeName.Remove(typeName.IndexOf('`'))}.{methodBase.Name}";
            var methodHashCode = Utilities.CommonHelper.GetInvariantHashCode(methodName);

            var logProperties = new[]
            {
                new LogProperty(LogConstants.LogTiming, null),
                new LogProperty(LogConstants.MethodName, methodName),
                new LogProperty(LogConstants.ElapsedMilliSeconds, milliseconds),
                new LogProperty(LogConstants.MethodHashCode, methodHashCode),
            };

            Logger.Log(LogLevel.Information, null, $"The \"{methodName}\" method elapsed in {milliseconds} MS.", null, logProperties);
#endif
        }
    }
}