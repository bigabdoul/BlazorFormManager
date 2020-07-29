// Copyright (c) Karfamsoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Components;

namespace BlazorFormManager.Debugging
{
    /// <summary>
    /// Specifies options on how to handle debug information related to JavaScript's XMLHttpRequest object.
    /// </summary>
    public class FormManagerXhrDebug
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormManagerXhrDebug"/> class.
        /// </summary>
        public FormManagerXhrDebug()
        {
        }

        /// <summary>
        /// Indicates, when applicable, whether to interpret HTTP responses 
        /// with the <see cref="MarkupString"/> structure or not. This could
        /// be potentially dangerous if the server is not trustworthy.
        /// </summary>
        public bool HtmlDocViewEnabled { get; set; }
    }
}
