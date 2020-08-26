using BlazorFormManager.ComponentModel.ViewAnnotations;
using System.Linq;

namespace BlazorFormManager.IO
{
    /// <summary>
    /// Represents the result of a file reading operation.
    /// </summary>
    public sealed class FileReaderResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileReaderResult"/> class.
        /// </summary>
        public FileReaderResult()
        {
        }

        /// <summary>
        /// Gets or sets a value that indicates whether a file reading operation was
        /// successful.
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// Gets or sets the input file identifier.
        /// </summary>
        public string InputId { get; set; }

        /// <summary>
        /// Gets or sets the input file name.
        /// </summary>
        public string InputName { get; set; }

        /// <summary>
        /// Gets or sets a file read operation result.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets a file read operation result as a buffer array.
        /// </summary>
        public int[] ContentArray { get; set; }

        /// <summary>
        /// Gets or sets the method used to read a file.
        /// </summary>
        public FileReaderMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the error that occurred during a file reading operation.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Casts the <see cref="ContentArray"/> to a-dimensional array of <see cref="byte"/> elements.
        /// </summary>
        /// <returns></returns>
        public byte[] ContentArrayAsByteArray() => ContentArray.Select(b => (byte)b).ToArray();

        /// <summary>
        /// Indicates whether the UI interaction was done in JavaScript.
        /// </summary>
        public bool CompletedInScript { get; set; }
    }
}