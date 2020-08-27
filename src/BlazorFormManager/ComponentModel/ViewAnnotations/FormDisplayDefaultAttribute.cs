using System;

namespace BlazorFormManager.ComponentModel.ViewAnnotations
{
    /// <summary>
    /// Represents a custom attributes that encapsulates default 
    /// values for an instance of the <see cref="FormDisplayAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FormDisplayDefaultAttribute : FormAttributeBase
    {
        internal static readonly FormDisplayDefaultAttribute Empty = new FormDisplayDefaultAttribute();

        /// <summary>
        /// Initializes a new instance of the <see cref="FormDisplayDefaultAttribute"/> class.
        /// </summary>
        public FormDisplayDefaultAttribute()
        {
        }

        /// <summary>
        /// Gets or sets the CSS class (e.g. row) for a section element.
        /// The default value is "row".
        /// </summary>
        public string GroupCssClass { get; set; } = "row";

        /// <summary>
        /// Gets or sets the CSS class (e.g. col) for an HTML element wrapped around an input.
        /// The default value is "col".
        /// </summary>
        public string ColumnCssClass { get; set; } = "col";

        /// <summary>
        /// Gets or sets the CSS class (e.g. form-control) added to the input.
        /// The default value is "form-control".
        /// </summary>
        public string InputCssClass { get; set; } = "form-control";

        /// <summary>
        /// Indicates whether to display the name of a group.
        /// </summary>
        public bool ShowGroupName { get; set; }

        /// <summary>
        /// Indicates whether rendering custom inputs is disabled, enabled or determined
        /// by the default value. The default is <see cref="CustomRenderMode.Enabled"/>.
        /// </summary>
        public CustomRenderMode CustomRenderMode { get; set; } = CustomRenderMode.Enabled;

        /// <summary>
        /// Creates a new instance of the <see cref="FormDisplayAttribute"/> from matching 
        /// properties of the current <see cref="FormDisplayDefaultAttribute"/> instance.
        /// </summary>
        /// <returns></returns>
        internal FormDisplayAttribute CreateDefault()
        {
            return new FormDisplayAttribute
            {
                ColumnCssClass = ColumnCssClass,
                InputCssClass = InputCssClass,
                GroupName = string.Empty,
                CustomRenderMode = CustomRenderMode,
            };
        }
    }
}
