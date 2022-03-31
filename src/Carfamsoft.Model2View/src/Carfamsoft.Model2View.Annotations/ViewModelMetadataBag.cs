using Carfamsoft.Model2View.Shared;

namespace Carfamsoft.Model2View.Annotations
{
    /// <summary>
    /// Represents an object that encapsulates metadata for a model.
    /// </summary>
    public class ViewModelMetadataBag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelMetadataBag"/> class.
        /// </summary>
        public ViewModelMetadataBag()
        {
        }

        /// <summary>
        /// Gets or sets the view model to render.
        /// </summary>
        public object ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the type of the <see cref="ViewModel"/> property.
        /// </summary>
        public System.Type ViewModelType { get; set; }

        /// <summary>
        /// Gets or sets the metadata retrieved from <see cref="ViewModel"/>.
        /// </summary>
        public AutoInputMetadata Metadata { get; set; }

        /// <summary>
        /// Gets or sets the render options.
        /// </summary>
        public ControlRenderOptions RenderOptions { get; set; }

        /// <summary>
        /// Gets or sets the alignment for labels.
        /// </summary>
        public ContentAlignment LabelAlignment { get; set; } = ContentAlignment.Left;

        /// <summary>
        /// Gets or sets a custom object that represents the label.
        /// </summary>
        public object LabelContent { get; set; }

        /// <summary>
        /// Gets or sets a custom object that represents the child content.
        /// </summary>
        public object ChildContent { get; set; }

        /// <summary>
        /// Gets or sets a custom object that represents the validation content.
        /// </summary>
        public object ValidationContent { get; set; }

        /// <summary>
        /// Gets or sets the parent property name, which is prefixed to an HTML element's name.
        /// </summary>
        public string ParentPropertyName { get; set; }

        /// <summary>
        /// Gets or sets a custom object.
        /// </summary>
        public object Tag { get; set; }
    }
}
