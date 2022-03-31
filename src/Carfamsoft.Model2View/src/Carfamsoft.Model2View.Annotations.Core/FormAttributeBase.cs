namespace Carfamsoft.Model2View.Annotations
{
    /// <summary>
    /// Represents the base class for form-related custom attributes.
    /// </summary>
    public abstract class FormAttributeBase : System.Attribute
    {
        /// <summary>
        /// Intializes a new instance of the <see cref="FormAttributeBase"/> class.
        /// </summary>
        protected FormAttributeBase()
        {
        }

        /// <summary>
        /// Gets or sets the CSS class (e.g. form-control) added to the input.
        /// </summary>
        public virtual string InputCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value that will be used to set the watermark for prompts in the UI.
        /// </summary>
        public virtual string Prompt { get; set; }

        /// <summary>
        /// Indicates whether the input is disabled.
        /// </summary>
        public virtual bool Disabled { get; set; }

        /// <summary>
        /// Indicates whether this custom attribute should be ignored during rendering.
        /// </summary>
        protected internal bool Ignore { get; set; }

        /// <summary>
        /// Determines whether this attribute should be ignored.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsIgnored() => Ignore;
    }
}
