// Copyright (c) Karfamsoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BlazorFormManager.Debugging;

namespace BlazorFormManager
{
    /// <summary>
    /// Encapsulates information about the outcome of a form submission.
    /// </summary>
    public sealed class FormManagerSubmitResult
    {
        private FormManagerSubmitResult(bool succeeded, FormManagerXhrResult xhr, string message, bool uploadContainedFiles)
        {
            XHR = xhr;
            Message = message;
            Succeeded = succeeded;
            UploadContainedFiles = uploadContainedFiles;
        }

        /// <summary>
        /// Gets information about the XMLHttpRequest object that was involved in the request.
        /// </summary>
        public FormManagerXhrResult XHR { get; }

        /// <summary>
        /// Indicates whether the form submission was successful or not.
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// Indicates whether the upload contained one or more files.
        /// </summary>
        public bool UploadContainedFiles { get; }

        /// <summary>
        /// Gets or sets the error or success message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Creates and initializes a new instance of the <see cref="FormManagerSubmitResult"/>
        /// class for a successful form submission.
        /// </summary>
        /// <param name="xhr">Information about the XMLHttpRequest object that was involved in the request.</param>
        /// <param name="message">The success message.</param>
        /// <param name="uploadContainedFiles">Indicates whether the upload contained one or more files.</param>
        /// <returns></returns>
        public static FormManagerSubmitResult Success(FormManagerXhrResult xhr, string message, bool uploadContainedFiles)
            => new FormManagerSubmitResult(true, xhr, message, uploadContainedFiles);

        /// <summary>
        /// Creates and initializes a new instance the <see cref="FormManagerSubmitResult"/>
        /// class using the specified <paramref name="result"/> and <paramref name="message"/>.
        /// </summary>
        /// <param name="result">The result to derive from.</param>
        /// <param name="message">The success message.</param>
        /// <returns></returns>
        public static FormManagerSubmitResult Success(FormManagerSubmitResult result, string message = null)
            => new FormManagerSubmitResult(true, result.XHR, message ?? result.Message, result.UploadContainedFiles);

        /// <summary>
        /// Creates and initializes a new instance of the <see cref="FormManagerSubmitResult"/>
        /// class for a non successful form submission.
        /// </summary>
        /// <param name="xhr">Information about the XMLHttpRequest object that was involved in the request.</param>
        /// <param name="message">The error message.</param>
        /// <param name="uploadContainedFiles">Indicates whether the upload contained one or more files.</param>
        /// <returns></returns>
        public static FormManagerSubmitResult Failed(FormManagerXhrResult xhr, string message, bool uploadContainedFiles)
            => new FormManagerSubmitResult(false, xhr, message, uploadContainedFiles);

        /// <summary>
        /// Creates and initializes a new instance the <see cref="FormManagerSubmitResult"/>
        /// class using the specified <paramref name="result"/> and <paramref name="message"/>.
        /// </summary>
        /// <param name="result">The result to derive from.</param>
        /// <param name="message">The error message.</param>
        public static FormManagerSubmitResult Failed(FormManagerSubmitResult result, string message = null)
            => new FormManagerSubmitResult(false, result.XHR, message ?? result.Message, result.UploadContainedFiles);
    }
}
