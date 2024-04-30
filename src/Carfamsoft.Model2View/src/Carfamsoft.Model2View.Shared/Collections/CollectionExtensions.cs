using System;
using System.Collections.Generic;

namespace Carfamsoft.Model2View.Shared.Collections
{
    /// <summary>
    /// Provides extension methods for collection.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Merges the <paramref name="source"/> collection with the <paramref name="target"/> dictionary
        /// and returns the target dictionary.
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
        /// Sets the values of this dictionary by converting values from the given dictionary.
        /// </summary>
        /// <typeparam name="TSourceKey"></typeparam>
        /// <typeparam name="TTargetValue"></typeparam>
        /// <typeparam name="TSourceValue"></typeparam>
        /// <param name="target">The target dictionary.</param>
        /// <param name="source">The dictionary to read values from.</param>
        /// <param name="converter">
        /// A function to convert each value contained in the source dictionary.
        /// </param>
        /// <param name="skipIf">
        /// A function that returns true when a dictionary entry should be skipped.
        /// </param>
        /// <returns></returns>
        public static IDictionary<TSourceKey, TTargetValue> ConvertMerge<TSourceKey, TTargetValue, TSourceValue>
        (
            this IDictionary<TSourceKey, TTargetValue> target,
#nullable enable
            IDictionary<TSourceKey, TSourceValue?> source, Func<TSourceKey, TSourceValue?, TTargetValue> converter, Func<TSourceKey, TSourceValue?, bool>? skipIf = null
#nullable disable
        )
        {
            foreach (var key in source.Keys)
            {
                var srcValue = source[key];

                if (skipIf == null || !skipIf.Invoke(key, srcValue))
                    target[key] = converter.Invoke(key, srcValue);
            }

            return target;
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
