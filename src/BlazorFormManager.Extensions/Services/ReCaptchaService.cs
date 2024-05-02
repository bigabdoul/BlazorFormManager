using BlazorFormManager.Components.Forms;
using BlazorFormManager.Extensions.Configuration;
using BlazorFormManager.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace BlazorFormManager.Extensions.Services
{
    /// <summary>
    /// Represents a reCAPTCHA configuration service provider.
    /// </summary>
    /// <param name="configuration"></param>
    public class ReCaptchaService(IConfiguration configuration) : IReCaptchaService
    {
        private readonly IDictionary<string, IReCaptchaOptions> _recaptcha = configuration.ReadReCaptcha();

        /// <summary>
        /// Gets the version 2 configuration options.
        /// </summary>
        public IReCaptchaOptions Version2 => GetVersion("v2");

        /// <summary>
        /// Gets the version 3 configuration options.
        /// </summary>
        public IReCaptchaOptions Version3 => GetVersion("v3");

        /// <summary>
        /// Returns the configuration options for a specific reCAPTCHA version.
        /// </summary>
        /// <param name="key">The version key.</param>
        /// <returns></returns>
        /// <exception cref="ReCaptchaConfigurationException"></exception>
        public IReCaptchaOptions GetVersion(string key) =>
            _recaptcha.TryGetValue(key, out var value) ? value : throw new ReCaptchaConfigurationException();
    }
}
