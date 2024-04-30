using Carfamsoft.Model2View.Shared.Collections;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using CollectionExtensions = Carfamsoft.Model2View.Shared.Collections.CollectionExtensions;

namespace BlazorFormManager.Components.Web
{
    /// <summary>
    /// Represents an object that encapsulates information about a table header or data cell.
    /// </summary>
    public class TableCell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableCell"/> class.
        /// </summary>
        public TableCell()
        {
        }

        /// <summary>
        /// Gets or sets the name or identifier of the table cell.
        /// Not to be confused with the <see cref="Text"/> property.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the text displayed in the table cell.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets a text that describes the content of this table cell.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the CSS class to be applied to this table cell.
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Gets or sets the icon to display in front of the <see cref="Text"/>.
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Gets or sets the number of columns that this cell spans over.
        /// </summary>
        public int? ColSpan { get; set; }

        /// <summary>
        /// Gets or sets the number of rows that this cell spans over.
        /// </summary>
        public int? RowSpan { get; set; }

        /// <summary>
        /// Indicates whether the cell should be hidden or displayed.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or sets the cell's value. It can be anything, including an instance
        /// of the <see cref="MarkupString"/> structure.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Indicates whether the current cell, usually a table header, is not sorted
        /// (null), sorted in ascending (true) or descending (false) order.
        /// </summary>
        public bool? SortAscending { get; set; }

        /// <summary>
        /// Indicates whether the current cell is clickable or not.
        /// </summary>
        public bool NonClickable { get; set; }

        /// <summary>
        /// Gets or sets an event callback delegate to invoke when this cell is clicked.
        /// </summary>
        public EventCallback<MouseEventArgs> OnClick { get; set; }

        /// <summary>
        /// Gets or sets the table cell's data type.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Returns a dictionary containing the 'class', 'colspan', and 'title' attributes
        /// if <paramref name="data"/> is an instance of the <see cref="TableCell"/> class;
        /// otherwise, it returns null.
        /// </summary>
        /// <param name="data">The data to check.</param>
        /// <param name="value">
        /// Returns the <see cref="Value"/> or <see cref="Text"/> of <paramref name="data"/>
        /// if it's an instance of <see cref="TableCell"/>; otherwise, returns a reference to 
        /// <paramref name="data"/>.
        /// </param>
        /// <param name="onclick">
        /// Returns the event handler delegate if <paramref name="data"/> 
        /// is an instance of <see cref="TableCell"/>.
        /// </param>
        /// <returns></returns>
        public static IDictionary<string, object>? GetAttributes(object? data, out object? value, out EventCallback<MouseEventArgs> onclick)
        {
            if (data is TableCell cell)
            {
                value = cell.Value ?? cell.Text;
                onclick = cell.OnClick;

                return CollectionExtensions.GetAttributes(
                    ("class", cell.CssClass + (!string.IsNullOrWhiteSpace(cell.Type) ? " datatype-" + cell.Type!.ToLower() : null)),
                    ("colspan", cell.ColSpan),
                    ("rowspan", cell.RowSpan),
                    ("title", cell.Description));
            }
            else
            {
                onclick = EventCallback<MouseEventArgs>.Empty;
                value = data;
                var dataType = value?.GetType().Name.ToLower();
                if (!string.IsNullOrWhiteSpace(dataType))
                    dataType = $"datatype-{dataType}";
                return CollectionExtensions.GetAttributes(
                    ("class", dataType)
                );
            }
        }
    }
}
