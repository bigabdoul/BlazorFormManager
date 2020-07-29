// Copyright (c) Karfamsoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace BlazorFormManager
{
    /// <summary>
    ///  Encapsulates data related to a form upload event.
    /// </summary>
    public class UploadProgressChangedEventArgs : CancelEventArgs
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
        /// Gets or sets the upload progress event type.
        /// </summary>
        public UploadProgressEventType EventType { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes sent so far.
        /// </summary>
        public long BytesSent { get; set; }

        /// <summary>
        /// Gets or sets the total number of bytes to send.
        /// </summary>
        public long TotalBytesToSend { get; set; }

        /// <summary>
        /// Indicates whether a form upload contains files.
        /// </summary>
        public bool HasFiles { get; set; }

        /// <summary>
        /// Gets the upload progress percentage.
        /// </summary>
        public int ProgressPercentage => TotalBytesToSend > 0L
            ? Convert.ToInt32(100 * BytesSent / Convert.ToDouble(TotalBytesToSend))
            : 0;
    }
}
