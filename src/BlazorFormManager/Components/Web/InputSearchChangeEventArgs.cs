using BlazorFormManager.DOM;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorFormManager.Components.Web
{
    /// <summary>
    /// Rpresents an object that contains data related to an input search text changed event.
    /// </summary>
    public class InputSearchChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputSearchChangedEventArgs"/> class.
        /// </summary>
        public InputSearchChangedEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputSearchChangedEventArgs"/>
        /// class using the specified parameters.
        /// </summary>
        /// <param name="text">The actual search text.</param>
        /// <param name="eventType">The type of event that triggered the search text change.</param>
        /// <param name="keyboard">The keyboard event that triggered the change.</param>
        /// <param name="eventArgs">The generic JavaScript event that triggered the change.</param>
        public InputSearchChangedEventArgs(string? text, DomEventType eventType, KeyboardEventArgs? keyboard = null, JSEventArgs? eventArgs = null)
        {
            Text = text;
            EventType = eventType;
            Keyboard = keyboard;
            EventArgs = eventArgs;
        }

        /// <summary>
        /// Gets the actual value of the input.
        /// </summary>
        public string? Text { get; protected set; }

        /// <summary>
        /// Gets the type of event htat triggered the search text change.
        /// </summary>
        public DomEventType EventType { get; protected set; }

        /// <summary>
        /// Gets the keyboard event that triggered the change.
        /// </summary>
        public KeyboardEventArgs? Keyboard { get; protected set; }

        /// <summary>
        /// Gets the generic JavaScript event that triggered the change.
        /// </summary>
        public JSEventArgs? EventArgs { get; protected set; }
    }
}
