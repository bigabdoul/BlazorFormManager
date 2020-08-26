namespace BlazorFormManager.IO
{
    /// <summary>
    /// Provides data for cancelable file read/upload event.
    /// </summary>
    public sealed class FileReaderProgressChangedEventArgs : ProgressChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UploadProgressChangedEventArgs"/> class.
        /// </summary>
        public FileReaderProgressChangedEventArgs()
        {
        }

        /// <inheritdoc/>
        public FileReaderProgressChangedEventArgs(bool cancel) : base(cancel)
        {
        }

        /// <summary>
        /// Gets or sets the number of bytes read so far.
        /// </summary>
        public long BytesRead { get => BytesReadOrSent; set => BytesReadOrSent = value; }

        /// <summary>
        /// Gets or sets the total number of bytes to read.
        /// </summary>
        public long TotalBytesToRead { get => TotalBytesToReadOrSend; set => TotalBytesToReadOrSend = value; }
    }
}