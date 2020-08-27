using System;

namespace BlazorFormManager.ComponentModel.ViewAnnotations
{
    /// <summary>
    /// Represents the base class for custom attributes.
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
        /// Gets or sets a value that will be used to set the watermark for prompts in the UI.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// Gets or sets the type that contains the resources for the <see cref="ShortName"/>,
        /// <see cref="Name"/>, <see cref="Prompt"/>, and <see cref="Description"/> properties.
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// Indicates whether the input is disabled.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Indicates whether to enable support for dragging and dropping files onto 
        /// a target element.
        /// </summary>
        public bool EnableDragDrop { get; set; }

        /// <summary>
        /// Gets or sets the identifier of an element that supports drag and drop.
        /// </summary>
        public string DropTargetElementId { get; set; }

        /// <summary>
        /// Gets or sets the icon that identifies a drag and drop operation. The default is 'copy'.
        /// </summary>
        public string DropEffect { get; set; } = "copy";
    }
}
