using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SmartCacheManager.Utilities
{
    public static class EnumHelper
    {
        private static readonly ConcurrentDictionary<Type, bool> FlagableEnums = new ConcurrentDictionary<Type, bool>();

        /// <summary>
        /// Determines whether enum is flagable
        /// </summary>
        /// <typeparam name="TEnum">Type of enum</typeparam>
        /// <returns>True if enum is flagable</returns>
        public static bool IsFlagable<TEnum>() where TEnum : Enum
        {
            return FlagableEnums.GetOrAdd(typeof(TEnum), type =>
            {
                return Enum.GetValues(type).Cast<TEnum>()
                    .Select(p => Convert.ToInt32(p))
                    .All(IsBinarySequence);
            });

            bool IsBinarySequence(int i) => (i & (i - 1)) == 0;
        }

        /// <summary>
        /// Retrieves an array of the flaged (HasFlag) values of the specified enum TEnum
        /// </summary>
        /// <typeparam name="TEnum">Type of enumeration</typeparam>
        /// <returns>List of enum vlaues</returns>
        public static List<TEnum> GetFlagedValues<TEnum>(this TEnum @enum) where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Where(p => @enum.HasFlag(p)).ToList();
        }
    }
}
