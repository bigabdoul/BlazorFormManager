using System.Collections.Generic;
using System.Linq;

namespace Carfamsoft.Model2View.Shared.Extensions
{
    /// <summary>
    /// Provides extension methods for instances of the <see cref="SelectOption"/> and <see cref="SelectOptionList"/> classes.
    /// </summary>
    public static class SelectOptionExtensions
    {
        /// <summary>
        /// Sets the <see cref="SelectOption.IsPrompt"/> property to true where <paramref name="selectedId"/>
        /// matches the <see cref="SelectOption.Id"/> property of the the first occurrence in the collection.
        /// </summary>
        /// <param name="collection">The collection to search.</param>
        /// <param name="selectedId">The selected identifier.</param>
        /// <returns>
        /// null if <paramref name="collection"/> is null or empty or if no element passes the specified test; 
        /// otherwise, the first element in <paramref name="collection"/> that passes the specified test.
        /// </returns>
        public static SelectOption SetSelected(this IEnumerable<SelectOption> collection, object selectedId)
        {
            var selected = collection?.FirstOrDefault(op => op.Id == $"{selectedId}");
            if (selected != null) selected.IsPrompt = true;
            return selected;
        }

        /// <summary>
        /// Returns the first element of the collection whose <see cref="SelectOption.IsPrompt"/> value is true.
        /// </summary>
        /// <param name="collection">A collection of <see cref="SelectOption"/> to return an element from.</param>
        /// <returns>
        /// null if <paramref name="collection"/> is null or empty or if no element passes the specified test; 
        /// otherwise, the first element in <paramref name="collection"/> that passes the specified test.
        /// </returns>
        public static SelectOption GetSelected(this IEnumerable<SelectOption> collection) 
            => collection?.FirstOrDefault(op => op.IsPrompt);
    }
}
