namespace BlazorFormManager.Components
{
    /// <summary>
    /// Encapsulates data related to an image.
    /// </summary>
    public class ComponentImage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentImage"/> class.
        /// </summary>
        /// <param name="src">The source attribute value for image tag.</param>
        /// <param name="width">The width attribute value for image tag.</param>
        /// <param name="height">The height attribute value for image tag.</param>
        public ComponentImage(string src, int? width = null, int? height = null)
        {
            Src = src;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Gets or sets the source attribute of the image.
        /// </summary>
        public string Src { get; set; }

        /// <summary>
        /// Gets or sets the width attribute of the image.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the height attribute of the image.
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets the alt attribute of the image.
        /// </summary>
        public string? Alt { get; set; }

        /// <summary>
        /// Gets or sets the class attribute of the image.
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Gets or sets the style attribute of the image.
        /// </summary>
        public string? Style { get; set; }
    }
}
