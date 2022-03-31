namespace BlazorFormManager.IO
{
    /// <summary>
    /// Represents the result of a JavaScript file reading operation.
    /// </summary>
    public class FileReaderInvokeResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileReaderInvokeResult"/> class.
        /// </summary>
        public FileReaderInvokeResult()
        {
        }

        /// <summary>
        /// Indicates whether the registration was done successfully.
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// Gets or sets the error that might have occurred during the operation.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public int Code { get; set; }
    }
}