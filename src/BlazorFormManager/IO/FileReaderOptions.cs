using BlazorFormManager.ComponentModel.ViewAnnotations;

namespace BlazorFormManager.IO
{
    /// <summary>
    /// Represents options for reading a file in JavaScript.
    /// </summary>
    public class FileReaderOptions
    {
        /// <summary>
        /// Gets or sets the input file identifier from which to read.
        /// </summary>
        public string InputId { get; set; }

        /// <summary>
        /// Gets or sets the input file name.
        /// </summary>
        public string InputName { get; set; }

        /// <summary>
        /// Gets or sets the method to read the file.
        /// </summary>
        public FileReaderMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the acceptable file name extensions (e.g. .jpg, .png, .pdf, .*).
        /// To accept any file, specify '.*' (without the single quotes).
        /// </summary>
        public string Accept { get; set; }

        /// <summary>
        /// Gets or sets the type of file allowed to be picked.
        /// </summary>
        public string AcceptType { get; set; }

        /// <summary>
        /// Gets or sets the identifier of an HTML element (typically a &lt;img /> tag)
        /// that will display the image.
        /// </summary>
        public string TargetElementId { get; set; }

        /// <summary>
        /// Gets or sets the name of the target element's attribute name that will 
        /// receive the base64-encoded data URL.
        /// </summary>
        public string TargetElementAttributeName { get; set; }
    }
}