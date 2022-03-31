using BlazorFormManager.IO;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorFormManager.DOM
{
    /// <summary>
    /// Represents an object used to hold the data that is being dragged during a drag
    /// and drop operation. This is a more strongly-typed version of the <see cref="DataTransfer"/>
    /// class. It may hold one or more <see cref="DataTransferItem"/>, each of one or more
    /// data types. For more information about drag and drop, see HTML Drag and Drop API.
    /// </summary>
    public sealed class DomDataTransfer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomDataTransfer"/> class.
        /// </summary>
        public DomDataTransfer()
        {
        }

        /// <summary>
        /// Gets or sets the kinds of operations that are to be allowed.
        /// </summary>
        public EffectAllowed EffectAllowed { get; set; }

        /// <summary>
        /// Gets or sets the kind of operation that is currently selected. If the kind of
        /// operation isn't one of those that is allowed by the effectAllowed attribute,
        /// then the operation will fail.
        /// </summary>
        public DropEffect DropEffect { get; set; }

        /// <summary>
        /// Contains a list of all the local files available on the data transfer. If the
        /// drag operation doesn't involve dragging files, this property is an empty list.
        /// </summary>
        public InputFileInfo[]? Files { get; set; }

        /// <summary>
        /// Gives a <see cref="DataTransferItem"/> array which is a list of all of the drag data.
        /// </summary>
        public DataTransferItem[]? Items { get; set; }

        /// <summary>
        /// An array of <see cref="string"/> giving the formats that were set in the dragstart event.
        /// </summary>
        public string[]? Types { get; set; }
    }
}
