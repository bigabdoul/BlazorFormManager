using System.Collections.Generic;

namespace Carfamsoft.Model2View.Shared.Collections
{
    /// <summary>
    /// Represents a dictionary of objects whose keys are an object's 
    /// properties' name and values their corresponding values.
    /// </summary>
    public sealed class ObjectDictionary : Dictionary<string, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDictionary"/>
        /// class using the specified object.
        /// </summary>
        /// <param name="obj">A non-nullable object.</param>
        public ObjectDictionary(object obj) : this(obj, System.StringComparer.CurrentCulture)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDictionary"/>
        /// class using the specified object and string equality comparer.
        /// </summary>
        /// <param name="obj">An initialized object instance.</param>
        /// <param name="comparer"></param>
        public ObjectDictionary(object obj, IEqualityComparer<string> comparer) : base(FromObject(obj), comparer)
        {
        }

        /// <summary>
        /// Constructs a dictionary from the given object.
        /// </summary>
        /// <param name="obj">A collection of key/value pairs, or an initialized CLR object.</param>
        /// <returns></returns>
        public static
#if NET6_0_OR_GREATER
            IEnumerable<KeyValuePair<string, object>>
#else
            IDictionary<string, object>
#endif
            FromObject(object obj)
        {
#if NET6_0_OR_GREATER
            if (obj is IEnumerable<KeyValuePair<string, object>> collection)
                return collection;
#endif
            var dic = new Dictionary<string, object>();
            var props = obj.GetType().GetProperties();
            foreach (var pi in props) dic.Add(pi.Name, pi.GetValue(obj));
            return dic;
        }
    }
}
