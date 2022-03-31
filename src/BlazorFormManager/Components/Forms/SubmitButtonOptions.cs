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
        /// Indicates whether to center the text on the button.
        /// </summary>
        public bool CenterText { get; set; }
    }
}
