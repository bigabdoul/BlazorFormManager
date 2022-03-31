using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Carfamsoft.Model2View.Shared.Collections
{
    /// <summary>
    /// Provides extension methods used to search the properties' values of the
    /// objects contained within a collection.
    /// </summary>
    public static class PropertyValueSearcher
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> 
            _properties = new ConcurrentDictionary<Type, PropertyInfo[]>();

        /// <summary>
        /// Performs a simple textual property value search within the specified collection of objects.
        /// </summary>
        /// <typeparam name="TItem">The type of elements contained in the collection to search.</typeparam>
        /// <param name="collection">The collection of objects to search.</param>
        /// <param name="text">The text to search.</param>
        /// <param name="includeProperty">A function that filters out the desired properties to search.</param>
        /// <returns></returns>
        public static IReadOnlyCollection<TItem> SearchProperties<TItem>(this IEnumerable<TItem> collection, string text,
                                                                 Func<string, bool> includeProperty = null)
        {
            IReadOnlyCollection<TItem> result;

            if (collection == null || string.IsNullOrWhiteSpace(text) || collection.Count() == 0)
            {
                result = null;
            }
            else
            {
                var list = new List<TItem>();

                foreach (var item in collection)
                {
                    if (string.Join(" ", item.GetPropertyValues(includeProperty)).IndexOf(text, StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        list.Add(item);
                    }
                }

                result = list.AsReadOnly();
            }

            return result;
        }

        /// <summary>
        /// Returns a collection of values retrieved from the properties of the given object.
        /// </summary>
        /// <typeparam name="TItem">The type of the object to read the properties' values.</typeparam>
        /// <param name="obj">The object from which to read the properties' values.</param>
        /// <param name="includeProperty">A function that filters out the desired properties to search.</param>
        /// <returns></returns>
        public static IEnumerable<object> GetPropertyValues<TItem>(this TItem obj, Func<string, bool> includeProperty = null)
        {
            var noPredicate = includeProperty == null;

            foreach (var pi in GetProperties<TItem>())
            {
                if (noPredicate || includeProperty!.Invoke(pi.Name))
                {
                    yield return pi.GetValue(obj);
                }
            }
        }

        private static PropertyInfo[] GetProperties<TItem>()
        {
            if (_properties.TryGetValue(typeof(TItem), out var properties))
            {
                return properties;
            }

            properties = typeof(TItem).GetProperties().Where(p => p.CanRead).ToArray();
            _properties.TryAdd(typeof(TItem), properties);

            return properties;
        }
    }
}
