namespace BlazorFormManager.ComponentModel.ViewAnnotations
{
    /// <summary>
    /// Represents a custom attribute that is used to generate inputs of type file,
    /// provides settings for image file reading and automatic preview generation 
    /// using JavaScript's FileReader API.
    /// </summary>
    public class ImagePreviewAttribute : InputFileAttribute
    {
        /// <summary>
        /// Gets or sets the default suffix for <see cref="TargetElementId"/>.
        /// </summary>
        public static string TargetElementIdSuffix { get; set; } = "Preview";

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePreviewAttribute"/> class
        /// by setting the <see cref="InputFileAttribute.AcceptType"/> property value to
        /// 'image' and the <see cref="InputFileAttribute.Method"/> property value to
        /// <see cref="FileReaderMethod.ReadAsDataURL"/>.
        /// </summary>
        public ImagePreviewAttribute()
        {
            AcceptType = "image";
            Method = FileReaderMethod.ReadAsDataURL;
        }

        /// <summary>
        /// Gets or sets the identifier of an HTML element (typically a &lt;img /> tag)
        /// that will display the image. If this property's value is set, it can greatly
        /// improve performance of file reading operation as it avoids calling back 
        /// .NET managed code from JavaScript with a huge amount of base64-encoded URL.
        /// By convention, if this property's value is null, the name of the property
        /// that it decorates is used with the <see cref="TargetElementIdSuffix"/> value
        /// (e.g. the property 'Photo' will have a target element id of 'PhotoPreview').
        /// If the value is empty, then the target element id should not be used.
        /// </summary>
        public string TargetElementId { get; set; }

        /// <summary>
        /// Gets or sets the name of the target element's attribute name that will 
        /// receive the base64-encoded data URL. The default value is 'src'.
        /// </summary>
        public string TargetElementAttributeName { get; set; } = "src";
    }
}
