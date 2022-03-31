using System.Collections.Generic;

namespace Carfamsoft.Model2View.Shared.Collections
{
    /// <summary>
    /// Provides extension methods for collection.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Merges the <paramref name="source"/> collection with the <paramref name="target"/> dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="target">The target dictionary.</param>
        /// <returns></returns>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IDictionary<TKey, TValue> target)
        {
            if (source == null)
                return target;

            if (target == null)
                return source.ToDictionary();

            foreach (var kv in source)
                target[kv.Key] = kv.Value;

            return target;
        }

        /// <summary>
        /// Merges the <paramref name="source"/> dictionary with the <paramref name="target"/> dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="target">The target dictionary.</param>
        /// <returns></returns>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> target)
        {
            if (source == null)
                return target;

            if (target == null)
                return source;

            foreach (var kv in source)
                target[kv.Key] = kv.Value;

            return target;
        }

        /// <summary>
        /// Merges the <paramref name="source"/> dictionary with the <paramref name="target"/> collection.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source dictionary.</param>
        /// <param name="target">The target collection.</param>
        /// <returns></returns>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> target)
        {
            var mergedTarget = target.ToDictionary();

            if (source == null)
                return mergedTarget;

            if (mergedTarget == null)
                return source;

            foreach (var kv in source)
                mergedTarget[kv.Key] = kv.Value;

            return mergedTarget;
        }

        /// <summary>
        /// Transforms the specified collection into an instance of a class that 
        /// implements <see cref="IDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="collection">The collection to transform.</param>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{TKey}"/> implementation to use when
        /// comparing keys, or null to use the default <see cref="EqualityComparer{TKey}"/>
        /// for the type of the key.
        /// </param>
        /// <returns></returns>
        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer = null)
        {
            if (collection == null)
                return null;

            if (collection is IDictionary<TKey, TValue> src)
                return src;

            var dic = new Dictionary<TKey, TValue>(comparer);

            foreach (var kv in collection)
                dic.Add(kv.Key, kv.Value);

            return dic;
        }

        /// <summary>
        /// Makes sure that only HTML attributes with non-whitespace values are returned for the name/value tuples.
        /// </summary>
        /// <param name="parameters">A collection of name/value tuples.</param>
        /// <returns></returns>
        public static IDictionary<string, object> GetAttributes(params (string name, object value)[] parameters)
        {
            var attributes = new Dictionary<string, object>();

            if (parameters?.Length > 0)
                foreach (var (name, value) in parameters)
                    if (!string.IsNullOrWhiteSpace($"{value}"))
                        attributes.Add(name, value!);

            return attributes;
        }
    }
}
