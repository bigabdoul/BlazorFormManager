using System.Collections.Generic;

namespace BlazorFormManager.Components.Web
{
    /// <summary>
    /// Represents an object that encapsulates options for filtering keystrokes
    /// received from a keyboard.
    /// </summary>
    public class FilterKeyOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterKeyOptions"/>
        /// class.
        /// </summary>
        public FilterKeyOptions()
        {
        }

        /// <summary>
        /// Gets or sets the regular expressions pattern for allowed keys.
        /// </summary>
        public string? Pattern { get; set; }

        /// <summary>
        /// Gets or sets a collection of key codes to block.
        /// </summary>
        public IEnumerable<int>? BlockKeyCodes { get; set; }

        /// <summary>
        /// Gets or sets the type of keys allowed.
        /// </summary>
        public FilterKeyType AllowKeyType { get; set; }

        /// <summary>
        /// Indicates whether a static .NET callback method should be invoked 
        /// or not when an event passes through validation. By default, event
        /// callbacks are marshalled back to .NET. To disable this behaviour,
        /// you must explicitly opt-out by setting this property's value to 
        /// true.
        /// </summary>
        public bool NoCallbackOnPassThrough { get; set; }
    }
}
