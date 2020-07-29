// Copyright (c) Karfamsoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace BlazorFormManager
{
    /// <summary>
    /// Upload progress event types.
    /// </summary>
    public enum UploadProgressEventType
    {
        /// <summary>
        /// The upload has begun.
        /// </summary>
        Start = 0,

        /// <summary>
        /// Periodically delivered to indicate the amount of progress made so far.
        /// </summary>
        Progress = 1,

        /// <summary>
        /// The upload completed successfully.
        /// </summary>
        Complete = 2,

        /// <summary>
        /// The upload failed due to an error.
        /// </summary>
        Error = 3,

        /// <summary>
        /// The upload operation was aborted.
        /// </summary>
        Abort = 4,

        /// <summary>
        /// The upload timed out because a reply did not arrive within 
        /// the time interval specified by the XMLHttpRequest.timeout.
        /// </summary>
        Timeout = 5,

        /// <summary>
        /// The upload finished. This event does not differentiate between
        /// success or failure, and is sent at the end of the upload regardless
        /// of the outcome. Prior to this event, one of <see cref="Start"/>,
        /// <see cref="Error"/>, <see cref="Abort"/>, or <see cref="Timeout"/>
        /// will already have been delivered to indicate why the upload ended.
        /// </summary>
        End = 6,
    }
}
