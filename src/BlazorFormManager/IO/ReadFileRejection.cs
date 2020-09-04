namespace BlazorFormManager.IO
{
    /// <summary>
    /// Provides enumerated values for reasons a file reading operation was rejected.
    /// </summary>
    public enum ReadFileRejection
    {
        /// <summary>
        /// No reason.
        /// </summary>
        None = 0,

        /// <summary>
        /// File extension is not allowed.
        /// </summary>
        Extension = 1,

        /// <summary>
        /// File type is not allowed.
        /// </summary>
        Type = 2,

        /// <summary>
        /// Multiple files are not allowed.
        /// </summary>
        Multiple = 3,

        /// <summary>
        /// Operation was aborted.
        /// </summary>
        Aborted = 4,
    }
}