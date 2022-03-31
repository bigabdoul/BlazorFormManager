namespace BlazorFormManager.DOM
{
    /// <summary>
    /// Provides enumerated values for DOM event types.
    /// </summary>
    public enum DomEventType
    {
        /// <summary>
        /// The 'change' event.
        /// </summary>
        Change,

        /// <summary>
        /// The 'keydown' event. This event is cancelable.
        /// </summary>
        KeyDown,

        /// <summary>
        /// The 'keypress' event. This event is cancelable.
        /// </summary>
        KeyPress,

        /// <summary>
        /// The 'input' event. This event is NOT cancelable.
        /// </summary>
        Input,

        /// <summary>
        /// The 'keyup' event. This event is NOT cancelable.
        /// </summary>
        KeyUp,
    }
}
