namespace BlazorFormManager
{
    /// <summary>
    /// Represents an select element option.
    /// </summary>
    public class SelectOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectOption"/> class.
        /// </summary>
        public SelectOption()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectOption"/> class
        /// using the specified parameters.
        /// </summary>
        /// <param name="id">The option identifier.</param>
        /// <param name="value">The option value.</param>
        /// <param name="isPrompt">true if this is the default option; otherwise, false.</param>
        public SelectOption(object id, string value, bool isPrompt = false)
        {
            Id = $"{id}";
            Value = value;
            IsPrompt = isPrompt;
        }

        /// <summary>
        /// Gets or sets the identifier of the option.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the value of the option.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the option is the default.
        /// </summary>
        public bool IsPrompt { get; set; }
    }
}