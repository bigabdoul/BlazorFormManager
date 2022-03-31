using Microsoft.AspNetCore.Components.Web;

namespace BlazorFormManager.DOM
{
    /// <summary>
    /// Provides information about a mouse-related event.
    /// </summary>
    public class DomMouseEventArgs : MouseEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomMouseEventArgs"/> class.
        /// </summary>
        public DomMouseEventArgs()
        {
        }

        /// <summary>
        /// Indicates the X-movement coordinate.
        /// </summary>
        public double MovementX { get; set; }

        /// <summary>
        /// Indicates the Y-movement coordinate.
        /// </summary>
        public double MovementY { get; set; }

#if NETSTANDARD2_0
        /// <summary>
        /// Indicates the X-offset coordinate.
        /// </summary>
        public double OffsetX { get; set; }

        /// <summary>
        /// Indicates the Y-offset coordinate.
        /// </summary>
        public double OffsetY { get; set; }

        /// <summary>
        /// The X coordinate of the mouse pointer in page (DOM content) coordinates.
        /// </summary>
        public double PageX { get; set; }

        /// <summary>
        /// The Y coordinate of the mouse pointer in page (DOM content) coordinates.
        /// </summary>
        public double PageY { get; set; }
#endif
        /// <summary>
        /// Provides additional info about a key press.
        /// </summary>
        public long Which { get; set; }

        /// <summary>
        /// The X coordinate of the mouse pointer coordinates.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// The Y coordinate of the mouse pointer coordinates.
        /// </summary>
        public double Y { get; set; }
    }
}
