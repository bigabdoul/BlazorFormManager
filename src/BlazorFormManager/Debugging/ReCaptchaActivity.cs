// Copyright (c) Karfamsoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace BlazorFormManager.Debugging
{
    /// <summary>
    /// Represents an object that encapsulates information about a Google reCAPTCHA activity.
    /// </summary>
    public class ReCaptchaActivity
    {
        /// <summary>
        /// Gets or sets the message of the activity.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the activity (e.g. 'danger', 'warning', 'info', etc.)
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the activity data.
        /// </summary>
        public string? Data { get; set; }
    }
}
