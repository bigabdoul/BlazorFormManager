using System;
using System.ComponentModel;

namespace BlazorFormManager.IO
{
    /// <summary>
    /// Provides data for cancelable file read/upload event.
    /// </summary>
    public class ProgressChangedEventArgs : CancelEventArgs, IProgressTrack
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressChangedEventArgs"/> class.
        /// </summary>
        public ProgressChangedEventArgs()
        {
        }

        /// <inheritdoc/>
        public ProgressChangedEventArgs(bool cancel) : base(cancel)
        {
        }

        /// <summary>
        /// Gets or sets the read progress event type.
        /// </summary>
        public ProgressChangedEventType EventType { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes read or sent so far.
        /// </summary>
        public long BytesReadOrSent { get; set; }

        /// <summary>
        /// Gets or sets the total number of bytes to read or send.
        /// </summary>
        public long TotalBytesToReadOrSend { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Gets the upload or read progress percentage.
        /// </summary>
        public int ProgressPercentage => TotalBytesToReadOrSend > 0L
            ? Convert.ToInt32(100 * BytesReadOrSent / Convert.ToDouble(TotalBytesToReadOrSend))
            : 0;

        long IProgressTrack.Current { get => BytesReadOrSent; set => BytesReadOrSent = value; }
        long IProgressTrack.Total { get => TotalBytesToReadOrSend; set => TotalBytesToReadOrSend = value; }
    }
}