namespace BlazorFormManager.Components.Forms
{
    /// <summary>
    /// Encapsulates configuration properties for Google's reCAPTCHA.
    /// </summary>
    public class ReCaptchaOptions : IReCaptchaOptions
    {
        /// <summary>
        /// Gets or sets the reCAPTCHA (public) site key.
        /// </summary>
        public virtual string? SiteKey { get; set; }

        /// <summary>
        /// Gets or sets the form data key for reCAPTCHA when posting the form.
        /// This is the form collection key to lookup on the server in order to
        /// retrieve the token. The default value is "g-recaptcha-response".
        /// </summary>
        public virtual string VerificationTokenName { get; set; } = "g-recaptcha-response";

        /// <summary>
        /// Gets or sets the reCAPTCHA's version to use. The default 
        /// value is "v3". Supported versions are "v2" and "v3".
        /// </summary>
        public virtual string Version { get; set; } = "v3";

        /// <summary>
        /// Indicates whether to use reCAPTCHA on localhost.
        /// </summary>
        public virtual bool AllowLocalHost { get; set; }

        /// <summary>
        /// Gets or sets the theme to use. The default is light.
        /// </summary>
        public virtual string Theme { get; set; } = "light";

        /// <summary>
        /// Gets or sets the language code to use.
        /// </summary>
        public virtual string? LanguageCode { get; set; }

        /// <summary>
        /// Indicates whether to render an invisible reCAPTCHA (v2 only).
        /// If true, <see cref="Size"/> will be set to "invisible".
        /// </summary>
        public virtual bool Invisible { get; set; }

        /// <summary>
        /// Gets or sets the size of the widget. Supported values are "compact", 
        /// "normal", and "invisible". The default value is "normal".
        /// </summary>
        public virtual string Size { get; set; } = "normal";

        /// <summary>
        /// Gets or sets the CSS selector of the widget(s) to render.
        /// The default value is ".g-recaptcha".
        /// </summary>
        public virtual string CssSelector { get; set; } = ".g-recaptcha";
    }
}
