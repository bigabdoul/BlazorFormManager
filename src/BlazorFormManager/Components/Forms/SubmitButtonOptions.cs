namespace BlazorFormManager.Components.Forms
{
	/// <summary>
	/// Encapsulates properties for a submit button.
	/// </summary>
	public class SubmitButtonOptions
    {
        /// <summary>
        /// Gets or sets the button type (e.g. submit, button, search, etc.).
        /// The default value is submit.
        /// </summary>
        public string Type { get; set; } = "submit";

        /// <summary>
        /// Gets or sets the button's text.
        /// </summary>
        public string? Text { get; set; }
        
        /// <summary>
        /// Gets or sets the button's icon.
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Gets or sets the button's class.
        /// </summary>
        public string? Class { get; set; }

        /// <summary>
        /// Indicates whether to center the text on the button.
        /// </summary>
        public bool CenterText { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitButtonOptions"/> class.
        /// </summary>
        public SubmitButtonOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitButtonOptions"/> class
        /// using the specified parameters.
        /// </summary>
        /// <param name="text">The button's text.</param>
        /// <param name="icon">The button's icon.</param>
        /// <param name="centerText">Center the button text or not?</param>
        /// <param name="type">The button's type. Defaults to "submit".</param>
        public SubmitButtonOptions(string? text, string? icon = null, bool centerText = false, string type = "submit")
        {
            Text = text;
            Icon = icon;
            CenterText = centerText;
            Type = type;
        }
    }
}
