// Copyright (c) Karfamsoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;

namespace BlazorFormManager.Debugging
{
    /// <summary>
    /// Provides event data when a browser does not support AJAX upload with progress report.
    /// </summary>
    public class AjaxUploadNotSupportedEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AjaxUploadNotSupportedEventArgs"/> class.
        /// </summary>
        public AjaxUploadNotSupportedEventArgs()
        {
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="cancel"><inheritdoc/></param>
        public AjaxUploadNotSupportedEventArgs(bool cancel) : base(cancel)
        {
        }

        /// <summary>
        /// Gets or sets supplementary properties received from an XMLHttpRequest.
        /// </summary>
        public Dictionary<string, object>? ExtraProperties { get; set; }
    }
}
