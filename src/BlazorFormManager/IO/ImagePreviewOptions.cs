namespace BlazorFormManager.IO
{
    /// <summary>
    /// Encapsulates data used for generating an image preview from a file.
    /// </summary>
    public class ImagePreviewOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePreviewOptions"/> class.
        /// </summary>
        public ImagePreviewOptions()
        {
        }

        /// <summary>
        /// Indicates whether image previews should be automatically generated. The default value is true.
        /// </summary>
        public bool AutoGenerate { get; set; } = true;

        /// <summary>
        /// Indicates whether to include file metadata (name, size, etc.). The default value is true.
        /// </summary>
        public bool GenerateFileInfo { get; set; } = true;

        /// <summary>
        /// Gets or sets the name of the HTML element (typically a &lt;img /> tag)
        /// that will hold the image. The default value is "img".
        /// </summary>
        public string TagName { get; set; } = "img";

        /// <summary>
        /// Gets or sets the CSS class name for an auto-generated image.
        /// </summary>
        public string? TagClass { get; set; }

        /// <summary>
        /// Gets or sets the identifier of an HTML element (typically a &lt;img /> tag)
        /// that will display the image.
        /// </summary>
        public string? TagId { get; set; }

        /// <summary>
        /// Gets or sets the name of the target element's attribute name that will 
        /// receive the base64-encoded data URL. The default value is "src".
        /// </summary>
        public string AttributeName { get; set; } = "src";

        /// <summary>
        /// Gets or sets the DOM query selector for the element that is wrapped around
        /// the auto-generated image (e.g. '#auto-image-wrapper', '.image-wrapper', etc.)
        /// </summary>
        public string? WrapperSelector { get; set; }

        /// <summary>
        /// Gets or sets the preferred width of the thumbnail to be generated.
        /// If <see cref="PreserveAspectRatio"/> is true, it's enough to set only the <see cref="Width"/>.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the preferred height of the thumbnail to be generated.
        /// If <see cref="PreserveAspectRatio"/> is true, it's enough to set only the <see cref="Height"/>.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Indicates whether to disable dynamic image resizing.
        /// </summary>
        public bool NoResize { get; set; }

        /// <summary>
        /// Indicates whether to keep the original aspect ratio. The default value is true.
        /// </summary>
        public bool PreserveAspectRatio { get; set; } = true;
    }
}