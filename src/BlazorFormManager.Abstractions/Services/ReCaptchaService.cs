using BlazorFormManager.Components.Forms;

namespace BlazorFormManager.Services
{
    /// <summary>
    /// Represents a reCAPTCHA configuration service provider.
    /// </summary>
    public interface IReCaptchaService
    {
        /// <summary>
        /// Gets or sets the version 2 configuration options.
        /// </summary>
        IReCaptchaOptions Version2 { get; }

        /// <summary>
        /// Gets or sets the version 3 configuration options.
        /// </summary>
        IReCaptchaOptions Version3 { get; }

        /// <summary>
        /// Returns the configuration options for a specific reCAPTCHA version.
        /// </summary>
        /// <param name="key">The version key.</param>
        /// <returns></returns>
        IReCaptchaOptions GetVersion(string key);
    }
}
