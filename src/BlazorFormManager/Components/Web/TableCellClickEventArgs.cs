using Microsoft.AspNetCore.Components.Web;
using System;

namespace BlazorFormManager.Components.Web
{
    /// <summary>
    /// Represents an object used to encapsulate 
    /// information about a table cell click event.
    /// </summary>
    public sealed class TableCellClickEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableCellClickEventArgs"/> class
        /// using the specified parameters.
        /// </summary>
        /// <param name="name">The name of the header or data cell that was clicked.</param>
        /// <param name="sortAscending">Determines how sorting is done.</param>
        /// <param name="sortEnabled">Indicates whether sorting is currently enabled.</param>
        /// <param name="mouseEventArgs">The <see cref="MouseEventArgs"/> associated with the cell click.</param>
        /// <param name="sender">The object that raised the current event.</param>
        /// 
        public TableCellClickEventArgs(string name, bool? sortAscending, bool sortEnabled, MouseEventArgs mouseEventArgs,
            object sender)
        {
            Name = name;
            SortAscending = sortAscending;
            SortEnabled = sortEnabled;
            MouseEventArgs = mouseEventArgs;
            Sender = sender;
        }

        /// <summary>
        /// Gets the name of the header or data cell that was clicked.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Determines how sorting is done.
        /// </summary>
        public bool? SortAscending { get; }

        /// <summary>
        /// Indicates whether sorting is currently enabled.
        /// </summary>
        public bool SortEnabled { get; }

        /// <summary>
        /// Gets the <see cref="MouseEventArgs"/> associated with the cell click.
        /// </summary>
        public MouseEventArgs MouseEventArgs { get; }

        /// <summary>
        /// Gets the object that raised the current event.
        /// </summary>
        public object Sender { get; }
    }
}
