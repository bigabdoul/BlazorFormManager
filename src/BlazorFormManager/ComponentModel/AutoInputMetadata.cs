using BlazorFormManager.ComponentModel.ViewAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BlazorFormManager.ComponentModel
{
    /// <summary>
    /// Holds metadata required for rendering an auto-generated input component.
    /// </summary>
    public sealed class AutoInputMetadata
    {
        private object _value;
        private IAutoInputComponent _autoInputComponent;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoInputMetadata"/> class using the specified parameters.
        /// </summary>
        /// <param name="model">The model associated with an instance of an <see cref="IAutoInputComponent"/> component.</param>
        /// <param name="attr">An object that encapsulates display-related metadata.</param>
        /// 
        public AutoInputMetadata(object model, FormDisplayAttribute attr)
        {
            Attribute = attr ?? throw new ArgumentNullException(nameof(attr));
            PropertyInfo = attr.GetProperty() ?? throw new ArgumentNullException(nameof(PropertyInfo));
            Refresh(model ?? throw new ArgumentNullException(nameof(model)));
            ExtractOptionsFromRange();
        }

        /// <summary>
        /// Gets or sets the value of the input. This should be used with two-way binding.
        /// </summary>
        public object Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<object>.Default.Equals(value, _value))
                {
                    // Console.WriteLine($"Metadata value changed: {value}");
                    _value = value;
                    PropertyInfo.SetValue(Model, _value);
                    _autoInputComponent?.SetCurrentValue(_value);
                }
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="SelectOption"/> items for a 'select'
        /// or 'input' element of type 'radio' to generate.
        /// </summary>
        public IEnumerable<SelectOption> Options { get; private set; }

        /// <summary>
        /// Returns the display name for an input.
        /// </summary>
        /// <returns></returns>
        public string GetDisplayName() => Attribute.Name ?? ToSentence(PropertyInfo?.Name);

        /// <summary>
        /// Indicates whether the <see cref="FormDisplayAttribute.UITypeHint"/> property value is radio.
        /// </summary>
        public bool IsInputRadio => Attribute.UITypeHint == "radio";

        /// <summary>
        /// Indicates whether the input should be of type checkbox.
        /// </summary>
        public bool IsInputCheckbox => PropertyInfo.PropertyType.SupportsCheckbox(Attribute.UITypeHint);

        #region private/internal

        internal void Refresh(object model)
        {
            Model = model;
            if (model != null)
            {
                _value = PropertyInfo.GetValue(model);
            }
            else
            {
                _value = null;
            }
            _autoInputComponent?.SetCurrentValue(_value);
        }

        internal object Model { get; private set; }
        internal PropertyInfo PropertyInfo { get; }
        internal FormDisplayAttribute Attribute { get; }

        internal static string ToSentence(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            var sb = new System.Text.StringBuilder();

            sb.Append(char.ToUpper(s[0]));

            for (int i = 1; i < s.Length; i++)
            {
                if (char.IsUpper(s[i]) || char.IsDigit(s[i]))
                    sb.Append(' ');
                sb.Append(s[i]);
            }

            return sb.ToString();
        }

        internal void Attach(IAutoInputComponent autoInputBase)
        {
            _autoInputComponent = autoInputBase;
            _autoInputComponent.SetCurrentValue(_value);
        }

        private void ExtractOptionsFromRange()
        {
            if (Attribute.UIHint == "select" || Attribute.UIHint == "radio")
            {
                Options = PropertyInfo.GetCustomAttribute<RangeAttribute>().OptionsFromRange();
            }
        }

        #endregion
    }
}