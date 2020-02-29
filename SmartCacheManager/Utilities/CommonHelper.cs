using System;
using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SmartCacheManager.Utilities
{
    public static class CommonHelper
    {
        /// <summary>
        /// Converts a value to a destination type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="destinationType">The type to convert the value to.</param>
        /// <returns>The converted value.</returns>
        public static T ConvertTo<T>(this object value)
        {
            return (T)ConvertTo(value, typeof(T));
        }

        /// <summary>
        /// Converts a value to a destination type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="destinationType">The type to convert the value to.</param>
        /// <returns>The converted value.</returns>
        public static object ConvertTo(this object value, Type type)
        {
            return value.ConvertTo(type, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a destination type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="destinationType">The type to convert the value to.</param>
        /// <param name="culture">Culture</param>
        /// <returns>The converted value.</returns>
        public static object ConvertTo(this object value, Type destinationType, CultureInfo culture)
        {
            if (value == null)
                return null;

            destinationType.NotNull(nameof(destinationType));
            culture.NotNull(nameof(culture));

            //return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);

            var destinationConverter = TypeDescriptor.GetConverter(destinationType);
            if (destinationConverter.CanConvertFrom(value.GetType()))
                return destinationConverter.ConvertFrom(null, culture, value);

            var sourceType = value.GetType();
            var sourceConverter = TypeDescriptor.GetConverter(sourceType);
            if (sourceConverter.CanConvertTo(destinationType))
                return sourceConverter.ConvertTo(null, culture, value, destinationType);

            if (destinationType.IsEnum && value is int)
                return Enum.ToObject(destinationType, (int)value);

            if (value is IConvertible && !destinationType.IsInstanceOfType(value))
                return Convert.ChangeType(value, destinationType, culture);

            return value;
        }

        /// <summary>
        /// Returns the invariant hash code for this string.
        /// </summary>
        /// <param name="str">string to hash</param>
        /// <returns>Hash code</returns>
        public static long GetInvariantHashCode(this string str)
        {
            str.NotNull(nameof(str));
            using (var md5 = MD5.Create())
            {
                var hashed = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                return BitConverter.ToInt64(hashed, 0);
            }
        }

        /// <summary>
        /// Mark an exception as logged.
        /// </summary>
        /// <param name="exception">Exception to be mark</param>
        public static void MarkAsLogged(this Exception exception)
        {
            exception.NotNull(nameof(exception));
            exception.Data.Add(nameof(MarkAsLogged), null);
        }

        /// <summary>
        /// Check this exception is logged before
        /// </summary>
        /// <param name="exception"></param>
        /// <returns>Determines whether it is logged before</returns>
        public static bool IsLoggedBefore(this Exception exception)
        {
            exception.NotNull(nameof(exception));
            return exception.Data.Contains(nameof(MarkAsLogged));
        }
    }
}
