namespace BlazorFormManager.IO
{
    /// <summary>
    /// Provides enumerated values for a file reading event type.
    /// </summary>
    public enum ReadFileEventType
    {
        /// <summary>
        /// The operation has started.
        /// </summary>
        Start = 0,

        /// <summary>
        /// A file reading has been rejected.
        /// </summary>
        Rejected = 1,

        /// <summary>
        /// A file (not necessarily read) has been processed.
        /// </summary>
        Processed = 2,

        /// <summary>
        /// The operation completed, regardless of success or failure.
        /// </summary>
        End = 3
    }
}