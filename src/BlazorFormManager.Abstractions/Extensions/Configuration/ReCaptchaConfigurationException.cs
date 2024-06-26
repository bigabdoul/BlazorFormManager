﻿using System;

namespace BlazorFormManager.Extensions.Configuration
{
    /// <summary>
    /// Represents errors that occur in reCAPTCHA configurations.
    /// </summary>
    [Serializable]
    public class ReCaptchaConfigurationException : Exception
    {
        /// <inheritdoc />
        public ReCaptchaConfigurationException()
        {
        }

        /// <inheritdoc />
        public ReCaptchaConfigurationException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public ReCaptchaConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}