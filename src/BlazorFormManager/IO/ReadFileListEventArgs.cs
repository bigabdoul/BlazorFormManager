namespace BlazorFormManager.IO
{
    /// <summary>
    /// Encapsulates data related to a file list read event.
    /// </summary>
    public sealed class ReadFileListEventArgs : ProgressChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadFileListEventArgs"/> class.
        /// </summary>
        public ReadFileListEventArgs()
        {
        }

        /// <inheritdoc/>
        public ReadFileListEventArgs(bool cancel) : base(cancel)
        {
        }

        /// <summary>
        /// Gets or sets the type of the reading event.
        /// </summary>
        public ReadFileEventType Type { get; set; }

        /// <summary>
        /// Gets or sets the reason of a file reading rejection.
        /// </summary>
        public ReadFileRejection Reason { get; set; }

        /// <summary>
        /// Gets or sets the file that was denied reading.
        /// </summary>
        public InputFileInfo File { get; set; }

        /// <summary>
        /// Gets or sets the files that were denied reading due to the 'multiple' 
        /// attribute restrictions.
        /// </summary>
        public InputFileInfo[] Files { get; set; }

        /// <summary>
        /// Gets or sets the number of files read so far.
        /// </summary>
        public long FilesRead { get => BytesReadOrSent; set => BytesReadOrSent = value; }

        /// <summary>
        /// Gets or sets the number of files prepared to be read.
        /// </summary>
        public long TotalFilesToRead { get => TotalBytesToReadOrSend; set => TotalBytesToReadOrSend = value; }
    }
}