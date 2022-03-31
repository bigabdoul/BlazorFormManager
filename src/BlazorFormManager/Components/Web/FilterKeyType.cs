namespace BlazorFormManager.Components.Web
{
    /// <summary>
    /// Provides enumerated values for allowed keystroke types.
    /// </summary>
    public enum FilterKeyType
    {
        /// <summary>
        /// No filtering.
        /// </summary>
        None,

        /// <summary>
        /// Only english alphabetical letters and the underscore _ symbol.
        /// </summary>
        Alpha,

        /// <summary>
        /// Only english alphabetical letters and digits ([0-9]) and the underscore _ symbol.
        /// </summary>
        AlphaNumeric,

        /// <summary>
        /// Only digits [0-9].
        /// </summary>
        Digits,
    }
}
