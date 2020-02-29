using SmartCacheManager.Utilities;

namespace SmartCacheManager.Logging
{
    /// <summary>
    /// Represent a log property.
    /// </summary>
    public class LogProperty
    {
        /// <summary>
        /// Construct a LogProperty.
        /// </summary>
        /// <param name="name">The name of the property. Must be non-empty.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="destructureObjects">
        /// Determines whether it must be destruct.
        /// If true, and the value is a non-primitive, non-array type, then the value will be converted to a structure; otherwise, unknown types will be converted to scalars, which are generally stored as strings.
        /// </param>
        public LogProperty(string name, object value, bool destructureObjects = false)
        {
            Name = name.NotNullOrWhiteSpace(nameof(name));
            Value = value;
            DestructureObjects = destructureObjects;
        }

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The value of the property.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Determines whether it must be destruct.
        /// If true, and the value is a non-primitive, non-array type, then the value will be converted to a structure; otherwise, unknown types will be converted to scalars, which are generally stored as strings.
        /// </summary>
        public bool DestructureObjects { get; }
    }
}