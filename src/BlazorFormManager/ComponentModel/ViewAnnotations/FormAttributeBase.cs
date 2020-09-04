using System;

namespace BlazorFormManager.ComponentModel.ViewAnnotations
{
    /// <summary>
    /// Represents the base class for form-related custom attributes.
    /// </summary>
    public abstract class FormAttributeBase : Attribute
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
        public string InputCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value that will be used to set the watermark for prompts in the UI.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// Indicates whether the input is disabled.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Indicates whether this custom attribute should be ignored during rendering.
        /// </summary>
        protected internal bool Ignore { get; set; }
    }
}
