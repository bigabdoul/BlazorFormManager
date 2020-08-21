using System;
using System.Reflection;

namespace BlazorFormManager.ComponentModel.ViewAnnotations
{
    /// <summary>
    /// Represents a custom attribute that specifies layout for a form input.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class FormDisplayAttribute : Attribute
    {
        /// <summary>
        /// Provides a value to use for default form display options.
        /// </summary>
        public static readonly FormDisplayAttribute Empty = new FormDisplayAttribute
        {
            GroupName = string.Empty,
            ColumnCssClass = FormDisplayDefaultAttribute.Empty.ColumnCssClass,
            InputCssClass = FormDisplayDefaultAttribute.Empty.InputCssClass,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FormDisplayAttribute"/> class.
        /// </summary>
        public FormDisplayAttribute()
        {
        }

        /// <summary>
        /// Gets or sets a value that is used to group fields in the UI.
        /// </summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value that is used for display in the UI.
        /// Set to an empty string (value of <see cref="string.Empty"/>, not null) to omit the label.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value that is used for the grid column label.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        ///  Gets or sets a value that is used to display a description in the UI.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value that will be used to set the watermark for prompts in the UI.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// Gets or sets the order weight of the column.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the type that contains the resources for the <see cref="ShortName"/>,
        /// <see cref="Name"/>, <see cref="Prompt"/>, and <see cref="Description"/> properties.
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the CSS class (e.g. col-md-4) used for an element (column)
        /// wrapped around the input.
        /// </summary>
        public string ColumnCssClass { get; set; }

        /// <summary>
        /// Gets or sets the CSS class (e.g. form-control) added to the input.
        /// </summary>
        public string InputCssClass { get; set; }

        /// <summary>
        /// Gets or sets a suggestion for the input element to generate.
        /// </summary>
        public string UIHint { get; set; }

        /// <summary>
        /// Gets or sets a suggestion for the input type (e.g. checkbox, radio) to generate.
        /// </summary>
        public string UITypeHint { get; set; }

        /// <summary>
        /// Gets or sets an icon associated with the generated input.
        /// </summary>
        public string Icon { get; set; }

        #region internal

        private PropertyInfo _property;

        // The following methods represent the getter and setter of the
        // otherwise public property named 'Property' of the current attribute,
        // unnecessarily exposing it to the outer world.

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> of the model's property that the current attribute decorates.
        /// </summary>
        /// <returns></returns>
        internal PropertyInfo GetProperty() => _property;

        /// <summary>
        /// Sets the <see cref="PropertyInfo"/> of the model's property that the current attribute decorates.
        /// </summary>
        /// <param name="value"></param>
        internal void SetProperty(PropertyInfo value) => _property = value;

        #endregion
    }
}
