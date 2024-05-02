namespace BlazorFormManager.Components.Forms
{
    /// <summary>
    /// Represents configuration properties for Google's reCAPTCHA.
    /// </summary>
    public interface IReCaptchaOptions
    {
        /// <summary>
        /// Indicates whether to use reCAPTCHA on localhost.
        /// </summary>
        bool AllowLocalHost { get; set; }

        /// <summary>
        /// Gets or sets the CSS selector of the widget(s) to render.
        /// </summary>
        string CssSelector { get; set; }

        /// <summary>
        /// Indicates whether to render an invisible reCAPTCHA (v2 only).
        /// If true, <see cref="Size"/> will be set to "invisible".
        /// </summary>
        bool Invisible { get; set; }

        /// <summary>
        /// Gets or sets the language code to use.
        /// </summary>
        string? LanguageCode { get; set; }

        /// <summary>
        /// Gets or sets the reCAPTCHA (public) site key.
        /// </summary>
        string? SiteKey { get; set; }

        /// <summary>
        /// Gets or sets the size of the widget. Supported values are "compact", 
        /// "normal", and "invisible". The default value is "normal".
        /// </summary>
        string Size { get; set; }

        /// <summary>
        /// Gets or sets the theme to use (e.g., 'light' or 'dark').
        /// </summary>
        string Theme { get; set; }

        /// <summary>
        /// Gets or sets the form data key for reCAPTCHA when posting the form.
        /// This is the form collection key to lookup on the server in order to
        /// retrieve the token.
        /// </summary>
        string VerificationTokenName { get; set; }

        /// <summary>
        /// Gets or sets the reCAPTCHA's version to use.
        /// </summary>
        string Version { get; set; }
    }
}