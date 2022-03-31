using System;
using System.Collections.Generic;

namespace BlazorFormManager.DOM
{
    /// <summary>
    /// Represents an object that contains event data related to JavaScript events.
    /// </summary>
    public class JSEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JSEventArgs"/> class.
        /// </summary>
        public JSEventArgs()
        {
        }

        /// <summary>
        /// Gets or sets the HTML element identifier that raised this event.
        /// </summary>
        public string? TargetId { get; set; }

        /// <summary>
        /// Gets or sets the DOM event type.
        /// </summary>
        public string? EventType { get; set; }

        /// <summary>
        /// Gets or sets the current value, if any, of the element that raised the event.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Gets or sets a collection of all event properties.
        /// </summary>
        public Dictionary<string, object>? Arguments { get; set; }
    }
}