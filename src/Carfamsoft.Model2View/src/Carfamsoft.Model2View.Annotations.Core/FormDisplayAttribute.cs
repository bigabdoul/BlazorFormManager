using Carfamsoft.Model2View.Shared;
using Carfamsoft.Model2View.Shared.Collections;
using Carfamsoft.Model2View.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Resources;

namespace Carfamsoft.Model2View.Annotations
{
    /// <summary>
    /// Represents a custom attribute that specifies layout for a form input.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FormDisplayAttribute : FormAttributeBase
    {
        #region fields

        private PropertyInfo _property;
        private Type _nullableUnderlyingType;

        #endregion

        /// <summary>
        /// Provides a value to use for default form display options.
        /// </summary>
        public static readonly FormDisplayAttribute Empty = new FormDisplayAttribute
        {
            GroupName = string.Empty,
            ColumnCssClass = FormDisplayDefaultAttribute.Empty.ColumnCssClass,
            InputCssClass = FormDisplayDefaultAttribute.Empty.InputCssClass,
            CustomRenderMode = FormDisplayDefaultAttribute.Empty.CustomRenderMode,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FormDisplayAttribute"/> class.
        /// </summary>
        public FormDisplayAttribute()
        {
            if (InputCssClass == "form-control" && (IsInputCheckbox || IsInputRadio))
                InputCssClass = null;


        }

        /// <summary>
        /// Gets or sets a value that is used to group fields in the UI.
        /// </summary>
        public virtual string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the icon for the group.
        /// </summary>
        public virtual string GroupIcon { get; set; }

        /// <summary>
        /// Gets or sets a value that describes the group.
        /// </summary>
        public virtual string GroupDescription { get; set; }

        /// <summary>
        /// Gets or sets a value that is used for display in the UI.
        /// Set to an empty string (value of <see cref="string.Empty"/>, not null) to omit the label.
        /// </summary>
        public virtual string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the input name.
        /// </summary>
        public virtual string InputName { get; set; }

        /// <summary>
        /// Gets or sets a value that is used for the grid column label.
        /// </summary>
        public virtual string ShortName { get; set; }

        /// <summary>
        ///  Gets or sets a value that is used to display a description in the UI.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// Gets or sets the type that contains the resources for the <see cref="ShortName"/>,
        /// <see cref="Name"/>, <see cref="FormAttributeBase.Prompt"/>, and <see cref="Description"/> properties.
        /// </summary>
        public virtual Type ResourceType { get; set; }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Gets or sets the component type used to render the property decorated by the current attribute.
        /// </summary>
        public virtual Type EditorType { get; set; }
#endif

        /// <summary>
        /// Gets or sets the order weight of the column.
        /// </summary>
        public virtual int Order { get; set; }

        /// <summary>
        /// Gets or sets the CSS class (e.g. col-md-4) used for an element (column)
        /// wrapped around the input.
        /// </summary>
        public virtual string ColumnCssClass { get; set; }

        /// <summary>
        /// Gets or sets a suggestion for the HTML element to generate. Supported
        /// elements are all standard HTML data collection elements such as input,
        /// select, textarea, etc. Other elements are not supported.
        /// </summary>
        public virtual string UIHint { get; set; }

        /// <summary>
        /// Gets or sets the tag or element name of the control to display.
        /// This is an alias for <see cref="UIHint"/>.
        /// </summary>
        public virtual string Tag { get => UIHint; set => UIHint = value; }

        /// <summary>
        /// Gets or sets a suggestion for the input type to generate. Supported types are
        /// all standard HTML input types (e. g. text, checkbox, number, date, file...).
        /// All type names must be in lower-case characters.
        /// </summary>
        public virtual string UITypeHint { get; set; }

        /// <summary>
        /// Gets or sets the type of the control to display.
        /// This is an alias for <see cref="UITypeHint"/>.
        /// </summary>
        public virtual string Type { get => UITypeHint; set => UITypeHint = value; }

        /// <summary>
        /// Gets or sets an icon associated with the generated input.
        /// </summary>
        public virtual string Icon { get; set; }

        /// <summary>
        /// Gets or sets the data format. If you want to format a <see cref="DateTime"/>
        /// or <see cref="DateTimeOffset"/>, you must specify a valid value for 
        /// <see cref="CultureName"/> and set <see cref="UITypeHint"/> to 'text'.
        /// </summary>
        public virtual string Format { get; set; }

        /// <summary>
        /// The case-insensitive name of a culture to use for conversions and formatting.
        /// </summary>
        public virtual string CultureName { get; set; }

        /// <summary>
        /// Determines the styles permitted in numeric string arguments that are passed to
        /// the Parse and TryParse methods of the integral and floating-point numeric types.
        /// </summary>
        public virtual System.Globalization.NumberStyles NumberStyles { get; set; }

        /// <summary>
        /// Gets or sets a vertical pipe-separated list of key/value pairs of options to render for 'select' tag. 
        /// </summary>
        public virtual string Options { get; set; }

        /// <summary>
        /// Gets or sets a vertical pipe-separated list of key/value pairs of additional attributes to display.
        /// </summary>
        /// <example>
        /// <code>[FormDisplay(Tag = "textarea", ExtraAttributes = "cols=3|rows=10")]</code>
        /// </example>
        public virtual string ExtraAttributes { get; set; }

        /// <summary>
        /// Gets or sets the placeholder value.
        /// </summary>
        public virtual string Placeholder { get; set; }

        /// <summary>
        /// Indicates whether rendering custom inputs is disabled, enabled or determined by the default value.
        /// </summary>
        public virtual CustomRenderMode CustomRenderMode { get; set; }

        /// <summary>
        /// Whether to render a checkbox as a switch.
        /// </summary>
        public virtual bool CheckSwitch { get; set; }

        /// <summary>
        /// Whether to inline the input where appropriate.
        /// </summary>
        public virtual bool CheckInline { get; set; }

        /// <summary>
        /// Indicates whether to omit the label for checkboxes and radios.
        /// </summary>
        public virtual bool CheckNoLabel { get; set; }

        /// <summary>
        /// Whether to enable a rich text editor for the generated element.
        /// </summary>
        public virtual bool RichText { get; set; }

        /// <summary>
        /// Indicates whether the <see cref="UITypeHint"/> property value is checkbox.
        /// </summary>
        public virtual bool IsInputCheckbox => UITypeHint.EqualsIgnoreCase("checkbox");

        /// <summary>
        /// Indicates whether the <see cref="UITypeHint"/> property value is radio.
        /// </summary>
        public virtual bool IsInputRadio => UITypeHint.EqualsIgnoreCase("radio");

        /// <summary>
        /// Returns true if either of <see cref="IsInputCheckbox"/> or <see cref="IsInputRadio"/> is true.
        /// </summary>
        public virtual bool IsInputCheckboxOrRadio => IsInputCheckbox || IsInputRadio;

        /// <summary>
        /// Returns true if either the <see cref="UITypeHint"/> property value is file
        /// or the <see cref="FileAttribute"/> property value is not null.
        /// </summary>
        public virtual bool IsInputFile => UITypeHint.EqualsIgnoreCase("file") || FileAttribute != null;

        /// <summary>
        /// Determines whether the specified <paramref name="type"/> corresponds
        /// to the <see cref="UITypeHint"/> property value.
        /// </summary>
        /// <param name="type">The type of the HTML element to compare to.</param>
        /// <returns></returns>
        public virtual bool Is(string type) => UITypeHint.EqualsIgnoreCase(type);

        /// <summary>
        /// Determines the frame to generate representing an HTML element.
        /// </summary>
        /// <param name="elementType">Returns the type of element to generate.</param>
        /// <returns></returns>
        public virtual string GetElement(out string elementType)
        {
            string element = null;

            elementType = UITypeHint.IsBlank()
                ? null
                : UITypeHint;

            if (UIHint.IsNotBlank())
                element = UIHint;

            if (elementType == null)
            {
                var pi = GetProperty();
                var dataType = pi.GetCustomAttribute<DataTypeAttribute>(true);

                if (dataType != null)
                {
                    elementType = dataType.GetControlType();
                    if (element == null && dataType.DataType == DataType.MultilineText)
                    {
                        element = "textarea";
                        elementType = null;
                    }
                }
                else if (pi.GetCustomAttribute<EmailAddressAttribute>(true) != null)
                    elementType = "email";
                else
                    // check for other custom attributes that might 
                    // give us a hint about the type of control to render
                    elementType = pi.PropertyType.GetControlType();

                if (elementType.IsBlank() && SupportsInputDate())
                    elementType = "date";
            }

            if (element == null)
                element = "input";

            return element;
        }

        /// <summary>
        /// Parses the <see cref="ExtraAttributes"/> property value into 
        /// an initialized dictionary instance.
        /// </summary>
        /// <returns></returns>
        public virtual IDictionary<string, object> GetExtraAttributes()
        {
            return ExtraAttributes.ParseKeyValuePairs().ToDictionary(StringComparer.OrdinalIgnoreCase);
        }

#region Intended for infrastructure support only

        /// <summary>
        /// Gets or sets the <see cref="InputFileAttribute"/> associated with the 
        /// property that this <see cref="FormDisplayAttribute"/> decorates.
        /// </summary>
        public virtual InputFileAttribute FileAttribute { get; private set; }

        /// <summary>
        /// Gets the <see cref="FileAttribute"/> property value.
        /// </summary>
        /// <returns></returns>
        public virtual InputFileAttribute GetFileAttribute() => FileAttribute;

        /// <summary>
        /// Sets the <see cref="FileAttribute"/> property value.
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetFileAttribute(InputFileAttribute value) => FileAttribute = value;

        /// <summary>
        /// Gets or sets the <see cref="DragDropAttribute"/> property value.
        /// </summary>
        public virtual DragDropAttribute DragDropAttribute { get; private set; }

        /// <summary>
        /// Sets the <see cref="FileAttribute"/> property value.
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetDragDropAttribute(DragDropAttribute value) => DragDropAttribute = value;

        // The following methods represent the getter and setter of the
        // otherwise public property named 'Property' of the current attribute,
        // unnecessarily exposing it to the outer world.

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> of the model's property that the current attribute decorates.
        /// </summary>
        /// <returns></returns>
        public virtual PropertyInfo GetProperty() => _property;

        /// <summary>
        /// Sets the <see cref="PropertyInfo"/> of the model's property that the current attribute decorates.
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetProperty(PropertyInfo value)
        {
            _property = value;
            if (_property == null)
            {
                _nullableUnderlyingType = null;
            }
            else
            {
                _nullableUnderlyingType = Nullable.GetUnderlyingType(_property.PropertyType);
            }
        }

        /// <summary>
        /// Returns an initialized instance of the <see cref="ResourceManager"/> 
        /// class for the <see cref="ResourceType"/> property.
        /// </summary>
        /// <returns></returns>
        public virtual ResourceManager GetResourceManager() => ResourceType == null ? null : new ResourceManager(ResourceType);

#endregion

#region helpers

        private bool SupportsInputDate()
        {
            return (_nullableUnderlyingType ?? _property.PropertyType).IsDate();
        }

#endregion
    }
}
