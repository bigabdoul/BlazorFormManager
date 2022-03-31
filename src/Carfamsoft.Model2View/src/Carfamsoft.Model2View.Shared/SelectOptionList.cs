using System.Collections.Generic;

namespace Carfamsoft.Model2View.Shared
{
    /// <summary>
    /// Represents a collection of <see cref="SelectOption"/> items.
    /// </summary>
    public class SelectOptionList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectOptionList"/> class.
        /// </summary>
        public SelectOptionList()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectOptionList"/> class
        /// using the specified parameter.
        /// </summary>
        /// <param name="propertyName">The associated model property to which this list belongs.</param>
        public SelectOptionList(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectOptionList"/>
        /// class using the specified collection.
        /// </summary>
        /// <param name="collection">An array of collections whose elements should be added to the end of the underlying list.</param>
        public SelectOptionList(params IEnumerable<SelectOption>[] collection) : this(null, collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectOptionList"/> class
        /// using the specified parameters.
        /// </summary>
        /// <param name="propertyName">The associated model property to which this list belongs.</param>
        /// <param name="collection">An array of collections whose elements should be added to the end of the underlying list.</param>
        public SelectOptionList(string propertyName, params IEnumerable<SelectOption>[] collection)
        {
            PropertyName = propertyName;

            var items = new List<SelectOption>();

            foreach (var col in collection)
            {
                items.AddRange(col);
            }

            Items = items;
        }

        /// <summary>
        /// Gets a collection of <see cref="SelectOption"/> elements.
        /// </summary>
        public List<SelectOption> Items { get; set; }

        /// <summary>
        /// Gets or sets the name of the associated model property for this list.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the option group name (e.g. &lt;optgroup>)
        /// </summary>
        public string GroupName { get; set; }
    }
}
