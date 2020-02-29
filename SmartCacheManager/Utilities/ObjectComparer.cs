using System;
using System.Collections;
using System.Linq;

namespace SmartCacheManager.Utilities
{
    /// <summary>
    /// Attribute to mark properties for deep comparison
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class CompareAttribute : Attribute
    {
    }

    public static class ObjectComparer
    {
        /// <summary>
        /// Deep comparison
        /// Exported it from https://gist.github.com/danielkillyevo/5443439 and imporved
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool DeepEquals(object left, object right)
        {
            //Compare the references
            if (ReferenceEquals(left, right) || Equals(left, right))
                return true;

            if (left is null || right is null)
                return false;

            //Compare the types
            if (left.GetType() != right.GetType())
                return false;

            //Get all property infos of the right object
            var propertyInfos = right.GetType().GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(CompareAttribute), false).Any()).ToList();

            //Compare the property values of the left and right object
            foreach (var propertyInfo in propertyInfos)
            {
                var othersValue = propertyInfo.GetValue(right);
                var currentsValue = propertyInfo.GetValue(left);

                //Compare the references
                if (ReferenceEquals(othersValue, currentsValue) || Equals(othersValue, currentsValue))
                    continue;

                if (othersValue is null || currentsValue is null)
                    return false;

                if (othersValue.GetType() != currentsValue.GetType())
                    return false;

                //Comparison if the property is an IEnumerable
                if (propertyInfo.PropertyType.IsEnumerable())
                {
                    var result = false;
                    foreach (var currentsItem in (IEnumerable)currentsValue)
                    {
                        foreach (var othersItem in (IEnumerable)othersValue)
                        {
                            //Recursively call the Equal method
                            var areEqual = DeepEquals(currentsItem, othersItem);
                            if (areEqual)
                            {
                                result = true;
                                break;
                            }
                        }
                    }
                    if (!result)
                        return false;
                }
                else
                {
                    //Comparison for properties of a non collection type

                    if (currentsValue.GetType().IsCustomType())
                    {
                        //values are complex/classes
                        //that's why we have to recursively call the DeepEquals methods
                        var areEqual = DeepEquals(currentsValue, othersValue);
                        if (!areEqual)
                            return false;
                    }
                    else
                    {
                        //values are primitive types
                        var areEqual = currentsValue.Equals(othersValue);
                        if (!areEqual)
                            return false;
                    }
                }
            }

            return true;
        }

        public static bool IsEnumerable(this Type type)
        {
            return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static bool IsCustomType(this Type type)
        {
            //return type.Assembly.GetName().Name != "mscorlib";
            return type.IsCustomValueType() || type.IsCustomReferenceType();
        }

        public static bool IsCustomValueType(this Type type)
        {
            return type.IsValueType && !type.IsPrimitive && type.Namespace?.StartsWith("System", StringComparison.Ordinal) == false;
        }

        public static bool IsCustomReferenceType(this Type type)
        {
            return !type.IsValueType && !type.IsPrimitive && type.Namespace?.StartsWith("System", StringComparison.Ordinal) == false;
        }
    }
}
