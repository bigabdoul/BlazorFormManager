using Microsoft.AspNetCore.Components.Web;

namespace BlazorFormManager.DOM
{
    /// <summary>
    /// Supplies more information about a keyboard event that is being raised.
    /// </summary>
    public class JSKeyboardEventArgs : KeyboardEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JSKeyboardEventArgs"/> class.
        /// </summary>
        public JSKeyboardEventArgs()
        {
        }

        /// <summary>
        /// Gets or sets the identifier of the target element that is raising this event.
        /// </summary>
        public string? TargetId { get; set; }

        /// <summary>
        /// Gets or sets the DOM event type being raised.
        /// </summary>
        public string? EventType { get; set; }

        /// <summary>
        /// Gets or sets the current value of the target element.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Gets or sets the 'charCode' property of a keyboard event.
        /// </summary>
        public long CharCode { get; set; }

        /// <summary>
        /// Gets or sets the 'keyCode' property of a keyboard event.
        /// </summary>
        public long KeyCode { get; set; }

        /// <summary>
        /// Gets or sets the 'which' property of a UI event.
        /// </summary>
        public long Which { get; set; }

        /// <summary>
        /// Gets or sets the 'detail' property of a UI event.
        /// </summary>
        public long Detail { get; set; }
    }
}