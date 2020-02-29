﻿using System;
using System.Collections;
using System.Linq;

namespace SmartCacheManager.Utilities
{
    public static class Check
    {
        /// <summary>
        /// Validates that obj is not null , otherwise throws an exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static T NotNull<T>(this T obj, string name, string message = null)
        {
            if (obj == null)
                throw new ArgumentNullException($"{name} ({typeof(T)}", message);
            return obj;
        }

        /// <summary>
        /// Validates that str is not null or white space , otherwise throws an exception.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="name"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string NotNullOrWhiteSpace(this string str, string name, string message = null)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentNullException(name, message);
            return str;
        }

        /// <summary>
        /// Validates that list is not null or empty , otherwise throws an exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static T NotNullOrEmpty<T>(this T obj, string name, string message = null) where T : IEnumerable
        {
            if (obj == null)
                throw new ArgumentNullException($"{name} ({typeof(T)}", message);

            if (!obj.Cast<object>().Any())
                throw new ArgumentException($"Argument {name} ({typeof(T)}) is empty. " + message, name);

            return obj;
        }

        /// <summary>
        /// Checks that the list is not empty (throw ArgumentException if list is empty)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static T NotEmpty<T>(this T obj, string name, string message = null) where T : IEnumerable
        {
            if (!obj.Cast<object>().Any())
                throw new ArgumentException($"Argument {name} ({typeof(T)}) is empty. " + message, name);
            return obj;
        }
    }
}
