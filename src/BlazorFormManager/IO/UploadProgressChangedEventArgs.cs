// Copyright (c) Karfamsoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace BlazorFormManager.IO
{
    /// <summary>
    ///  Encapsulates data related to a form upload event.
    /// </summary>
    public sealed class UploadProgressChangedEventArgs : ProgressChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UploadProgressChangedEventArgs"/> class.
        /// </summary>
        public UploadProgressChangedEventArgs()
        {
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="cancel"></param>
        public UploadProgressChangedEventArgs(bool cancel) : base(cancel)
        {
        }

        /// <summary>
        /// Gets or sets the number of bytes sent so far.
        /// </summary>
        public long BytesSent { get => BytesReadOrSent; set => BytesReadOrSent = value; }

        /// <summary>
        /// Gets or sets the total number of bytes to send.
        /// </summary>
        public long TotalBytesToSend { get => TotalBytesToReadOrSend; set => TotalBytesToReadOrSend = value; }

        /// <summary>
        /// Indicates whether a form upload contains files.
        /// </summary>
        public bool HasFiles { get; set; }
    }
}
