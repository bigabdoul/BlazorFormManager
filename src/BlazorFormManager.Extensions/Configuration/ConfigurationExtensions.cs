﻿using BlazorFormManager.Components.Forms;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace BlazorFormManager.Extensions.Configuration
{
    /// <summary>
    /// Provides extension methods for configuration objects.
    /// </summary>
    public static class ConfigurationExtensions
    {
        #region fields
        
        private const string ReCAPTCHA = "BlazorFormManager:ReCaptcha:";
        private const string Versions = ReCAPTCHA + "Versions";
        private const string VerificationTokenName = nameof(ReCaptchaOptions.VerificationTokenName);
        private const string AllowLocalHost = nameof(ReCaptchaOptions.AllowLocalHost);
        private const string Invisible = nameof(ReCaptchaOptions.Invisible);
        private const string LanguageCode = nameof(ReCaptchaOptions.LanguageCode);

        #endregion

        /// <summary>
        /// Reads all reCAPTCHA configuration settings except the <see cref="ReCaptchaOptions.SecretKey"/> value.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>
        /// An initialized instance of the <see cref="Dictionary{TKey, TValue}"/>
        /// containing <see cref="ReCaptchaOptions"/> values.
        /// </returns>
        /// <exception cref="ReCaptchaConfigurationException">No reCAPTCHA versions found.</exception>
        public static IDictionary<string, ReCaptchaOptions> ReadReCaptcha(this IConfiguration config)
        {
            var versions = config[Versions]?.Split(',', ';');

            if (versions.Any())
            {
                var dic = new Dictionary<string, ReCaptchaOptions>();
                
                // default values
                var defaultTokenName = config[ReCAPTCHA + VerificationTokenName];
                var defaultAllowLocalhost = config.GetValue(ReCAPTCHA + AllowLocalHost, false);
                var defaultIsInvisible = config.GetValue(ReCAPTCHA + Invisible, false);
                var defaultLanguageCode = config.GetValue(ReCAPTCHA + LanguageCode, string.Empty);

                if (string.IsNullOrWhiteSpace(defaultTokenName))
                    defaultTokenName = "g-recaptcha-response";

                foreach (var ver in versions)
                {
                    var v = ver.Trim();
                    var key = $"{ReCAPTCHA}{v}:";
                    var size = config.GetValue(key + nameof(ReCaptchaOptions.Size), "normal");
                    var isInvisible = config.GetValue(key + Invisible, defaultIsInvisible);

                    if (isInvisible)
                        size = "invisible";

                    dic.Add(v, new ReCaptchaOptions()
                    {
                        Version = v,
                        Size = size,
                        Invisible = isInvisible,
                        SiteKey = config[key + nameof(ReCaptchaOptions.SiteKey)],
                        Theme = config.GetValue(key + nameof(ReCaptchaOptions.Theme), "light"),
                        CssSelector = config.GetValue(key + nameof(ReCaptchaOptions.CssSelector), ".g-recaptcha"),
                        LanguageCode = config.GetValue(key + LanguageCode, defaultLanguageCode),
                        VerificationTokenName = config.GetValue(key + VerificationTokenName, defaultTokenName),
                        AllowLocalHost = config.GetValue(key + AllowLocalHost, defaultAllowLocalhost),
                    });
                }

                return dic;
            }
            
            throw new ReCaptchaConfigurationException("No reCAPTCHA versions found.");
        }

        /// <summary>
        /// Reads all reCAPTCHA versions and their corresponding secret keys from the provided configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>
        /// An initialized instance of the <see cref="Dictionary{TKey, TValue}"/>
        /// containing <see cref="ReCaptchaOptions"/> values.
        /// </returns>
        /// <exception cref="ReCaptchaConfigurationException">No reCAPTCHA versions found.</exception>
        public static IDictionary<string, string> ReadReCaptchaSecrets(this IConfiguration config)
        {
            var versions = config[Versions]?.Split(',', ';');

            if (versions.Any())
            {
                var dic = new Dictionary<string, string>();

                foreach (var ver in versions)
                {
                    var v = ver.Trim();
                    var key = $"{ReCAPTCHA}{v}:SecretKey";

                    dic.Add(v, config[key]);
                }

                return dic;
            }

            throw new ReCaptchaConfigurationException("No reCAPTCHA versions found.");
        }
    }
}
