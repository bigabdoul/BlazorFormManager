using BlazorFormManager.Components.UI;
using BlazorFormManager.DOM;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlazorFormManager.Components.Web
{
    /// <summary>
    /// Represents an object that encapsulates configuration options of an input search text element.
    /// </summary>
    public class InputSearchOptions
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InputSearchOptions"/> class.
        /// </summary>
        public InputSearchOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputSearchOptions"/> class
        /// using the specified parameters.
        /// </summary>
        /// <param name="eventReceiver">The object instance that receives the event. Can be null.</param>
        /// <param name="textChangedHandler">An event handler delegate invoked when a change in the input search occurs.</param>
        /// <param name="changeDetection">The way change is detected within the input search text.</param>
        public InputSearchOptions(object? eventReceiver = null,
                                  Func<InputSearchChangedEventArgs, Task>? textChangedHandler = null,
                                  DomEventType changeDetection = DomEventType.Input)
        {
            ChangeDetection = changeDetection;

            if (textChangedHandler != null)
                OnTextChanged = EventCallback.Factory.Create(eventReceiver ?? this, textChangedHandler);
        } 

        #endregion

        /// <summary>
        /// The default input search options.
        /// </summary>
        public static readonly InputSearchOptions Default = new();

        /// <summary>
        /// Indicates whether the embedded <see cref="DataPager"/> supports searching.
        /// The default value is true.
        /// </summary>
        public bool CanSearch { get; set; } = true;

        /// <summary>
        /// Gets the way change is detected within the input search text.
        /// The default value is <see cref="DomEventType.Input"/>.
        /// </summary>
        public DomEventType ChangeDetection { get; } = DomEventType.Input;

        /// <summary>
        /// Gets or sets an event handler delegate invoked when a change in the input search occurs.
        /// </summary>
        public EventCallback<InputSearchChangedEventArgs> OnTextChanged { get; set; }

        /// <summary>
        /// Gets or sets the search text.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the label text. The default value is "Search:".
        /// </summary>
        public string? Label { get; set; } = "Search:";

        /// <summary>
        /// Gets or sets the placeholder attribute value.
        /// </summary>
        public string? Placeholder { get; set; }

        /// <summary>
        /// Gets or sets the CSS class for the input element. The default value is "form-control".
        /// </summary>
        public string? CssClass { get; set; } = "form-control";

        /// <summary>
        /// Gets or sets the type of the &lt;input> element. The default value is "search".
        /// </summary>
        public string? InputType { get; set; } = "search";

        /// <summary>
        /// Gets or sets the size of the input element.
        /// </summary>
        public ComponentSize InputSize { get; set; } = ComponentSize.Md;

        /// <summary>
        /// Returns the <see cref="InputSize"/> property value in lower case.
        /// </summary>
        /// <returns></returns>
        public string? InputSizeString => InputSize.ToString().ToLower();

        /// <summary>
        /// Gets or sets the options for filtering keystrokes received from a keyboard.
        /// </summary>
        public FilterKeyOptions? KeyFilters { get; set; }
    }
}
