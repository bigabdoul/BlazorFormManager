// Copyright (c) Karfamsoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace BlazorFormManager.Debugging
{
    /// <summary>
    /// Provides enumeration values for HTTP response types 
    /// received through JavaScript's XMLHttpRequest object.
    /// </summary>
    public enum XMLHttpRequestResponseType
    {
        /// <summary>
        /// The response is a text in a DOMString object.
        /// </summary>
        Text,

        /// <summary>
        /// The response is a JavaScript ArrayBuffer containing binary data.
        /// </summary>
        ArrayBuffer,

        /// <summary>
        /// The response is a Blob object containing the binary data.
        /// </summary>
        Blob,

        /// <summary>
        /// The response is an HTML Document or XML XMLDocument, as 
        /// appropriate based on the MIME type of the received data.
        /// </summary>
        Document,

        /// <summary>
        /// The response is a JavaScript object created by 
        /// parsing the contents of received data as JSON.
        /// </summary>
        Json,

        /// <summary>
        /// The response is part of a streaming download; this response 
        /// type is only allowed for download requests, and is only 
        /// supported by Internet Explorer.
        /// </summary>
        MsStream,
    }
}
