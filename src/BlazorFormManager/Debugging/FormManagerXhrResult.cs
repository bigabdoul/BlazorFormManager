// Copyright (c) Karfamsoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BlazorFormManager.Debugging
{
    /// <summary>
    /// Provides debugging information and configuration values for 
    /// HTTP requests done with JavaScript's XMLHttpRequest object.
    /// </summary>
    public sealed class FormManagerXhrResult
    {
        #region fields

        private IReadOnlyDictionary<string, string> _headers;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FormManagerXhrResult"/> class.
        /// </summary>
        public FormManagerXhrResult()
        {
        }

        #endregion

        #region properties

        /// <summary>
        /// Indicates whether an event caller should quit further processing.
        /// This property is not part of the XMLHttpRequest object.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets the request headers.
        /// </summary>
        public IDictionary<string, object> RequestHeaders { get; set; }

        #endregion

        #region XMLHttpRequest properties

        /// <summary>
        /// Gets or sets the response's body content as an ArrayBuffer, Blob, 
        /// Document, JavaScript Object, or DOMString, depending on the value 
        /// of the request's responseType property.
        /// </summary>
        public object Response { get; set; }

        /// <summary>
        /// Gets or sets all the response headers, separated by CRLF, 
        /// as a string, or returns null if no response has been received. 
        /// If a network error happened, an empty string is returned.
        /// <para>Raw header string example:</para>
        /// <para>
        /// date: Fri, 24 Jul 2020 21:04:30 GMT\r\n
        /// content-encoding: gzip\r\n
        /// x-content-type-options: nosniff\r\n
        /// server: meinheld/0.6.1\r\n
        /// x-frame-options: DENY\r\n
        /// content-type: text/html; charset=utf-8\r\n
        /// connection: keep-alive\r\n
        /// strict-transport-security: max-age=63072000\r\n
        /// vary: Cookie, Accept-Encoding\r\n
        /// content-length: 6502\r\n
        /// x-xss-protection: 1; mode=block\r\n
        /// </para>
        /// </summary>
        public string ResponseHeaders { get; set; }

        /// <summary>
        /// Gets or set the text received from a server following a request being sent.
        /// </summary>
        public string ResponseText { get; set; }

        /// <summary>
        /// Gets or sets an enumerated string value specifying the type of data 
        /// contained in the response. It also lets the author change the response 
        /// type. If an empty string is set as the value of responseType, the 
        /// default value of text is used. Supported values are: arraybuffer, blob,
        /// document, json, text, ms-stream
        /// </summary>
        public string ResponseType { get; set; }

        /// <summary>
        /// The read-only XMLHttpRequest.responseURL property returns the serialized 
        /// URL of the response or the empty string if the URL is null. If the URL 
        /// is returned, any URL fragment present in the URL will be stripped away. 
        /// The value of responseURL will be the final URL obtained after any redirects.
        /// </summary>
        public string ResponseUrl { get; set; }

        /// <summary>
        /// The XMLHttpRequest.responseXML read-only property returns a Document containing 
        /// the HTML or XML retrieved by the request; or null if the request was unsuccessful, 
        /// has not yet been sent, or if the data can't be parsed as XML or HTML.
        /// </summary>
        public string ResponseXML { get; set; }

        /// <summary>
        /// The read-only XMLHttpRequest.status property returns the 
        /// numerical HTTP status code of the XMLHttpRequest's response.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// The read-only XMLHttpRequest.statusText property returns a DOMString containing 
        /// the response's status message as returned by the HTTP server. Unlike 
        /// XMLHTTPRequest.status which indicates a numerical status code, this property 
        /// contains the text of the response status, such as "OK" or "Not Found".
        /// If the request's readyState is in UNSENT or OPENED state, the value of statusText 
        /// will be an empty string.
        /// </summary>
        public string StatusText { get; set; }

        /// <summary>
        /// The XMLHttpRequest.withCredentials property is a Boolean that indicates 
        /// whether or not cross-site Access-Control requests should be made using 
        /// credentials such as cookies, authorization headers or TLS client certificates. 
        /// Setting withCredentials has no effect on same-site requests.
        /// </summary>
        public bool WithCredentials { get; set; }

        #endregion

        #region Helpers

        /// <summary>
        /// Gets or sets supplementary properties received from an XMLHttpRequest object.
        /// </summary>
        public Dictionary<string, object> ExtraProperties { get; set; }

        /// <summary>
        /// Determines whether the <see cref="Status"/> code indicates 
        /// success (integer between 200 and 299 inclusive).
        /// </summary>
        public bool IsSuccessStatusCode => Status > 199 && Status < 300;

        /// <summary>
        /// Gets a read-only dictionary of all the response headers.
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers 
            => _headers ?? (_headers = GetAllResponseHeaders());

        /// <summary>
        /// Determines whether the 'content-type' response header 
        /// value contains the word 'html' (case-insensitive lookup).
        /// </summary>
        public bool IsHtmlResponse => ResponseContentTypeContains("html");

        /// <summary>
        /// Determines whether the 'content-type' response header 
        /// value contains the word 'json' (case-insensitive lookup).
        /// </summary>
        public bool IsJsonResponse => ResponseContentTypeContains("json");

        /// <summary>
        /// Determines whether the 'content-type' response header 
        /// contains the specified value (case-insensitive lookup).
        /// </summary>
        /// <param name="value">The header value to lookup.</param>
        /// <returns></returns>
        public bool ResponseContentTypeContains(string value)
            => ResponseContentTypeContains(value, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Determines whether the 'content-type' response header contains
        /// the specified value using the specified string comparison.
        /// </summary>
        /// <param name="value">The header value to lookup.</param>
        /// <param name="comparisonType">
        /// One of the enumeration values that specifies the rules to use in the comparison.
        /// </param>
        /// <returns></returns>
        public bool ResponseContentTypeContains(string value, StringComparison comparisonType)
        {
            if (Headers.TryGetValue("content-type", out var type))
            {
                return type?.IndexOf(value, comparisonType) > -1;
            }
            return false;
        }

        /// <summary>
        /// Returns the <see cref="ResponseType"/> property as a well-known enumeration value.
        /// </summary>
        /// <returns></returns>
        public XMLHttpRequestResponseType ParseResponseType()
        {
            if (ResponseType == "ms-stream")
                return XMLHttpRequestResponseType.MsStream;

            if (string.IsNullOrWhiteSpace(ResponseType))
                return XMLHttpRequestResponseType.Text;

            if (Enum.TryParse<XMLHttpRequestResponseType>(ResponseType, true, out var result))
                return result;

            throw new ArgumentException(
                $"{nameof(ResponseType)} is not one of the named constants " +
                $"defined for the {nameof(XMLHttpRequestResponseType)} enumeration."
            );
        }

        /// <summary>
        /// Parse the <see cref="ResponseHeaders"/> property into 
        /// a collection of key-value pairs of string elements.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, string> GetAllResponseHeaders()
        {
            var dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(ResponseHeaders))
            {
                using (var sr = new System.IO.StringReader(ResponseHeaders))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        var index = line.IndexOf(':');
                        if (index > -1)
                        {
                            var name = line.Substring(0, index).TrimEnd();
                            var value = line.Substring(index + 1).TrimStart();
                            dic.Add(name, value);
                        }
                    }
                }
            }
            return new ReadOnlyDictionary<string, string>(dic);
        }

        #endregion
    }
}
