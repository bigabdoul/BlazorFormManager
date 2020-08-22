// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BlazorFormManager.ComponentModel;
using BlazorFormManager.ComponentModel.ViewAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFormManager.Components
{
    /// <summary>
    /// A base class for form input components generated on the fly.
    /// </summary>
    public class AutoInputBase : InputBase<object>, IAutoInputComponent
    {
        #region fields
        
        private Type _propertyType;
        private FormDisplayAttribute _metadataAttribute;
        private Type _nullableUnderlyingType;
        private string _stepAttributeValue; // Null by default, so only allows whole numbers as per HTML spec
        private CultureInfo _culture;
        private string _format;

        #endregion

        /// <summary>
        /// Gets or sets metadata for generating a form input.
        /// </summary>
        [Parameter] public AutoInputMetadata Metadata { get; set; }

        /// <summary>
        /// Gets or sets the child content to be rendering inside the element.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Gets the associated <see cref="FormManagerBase"/>.
        /// </summary>
        [CascadingParameter] protected FormManagerBase Form { get; set; }

        /// <inheritdoc/>
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);

            if (_propertyType == null)
            {
                if (Metadata == null) throw new ArgumentNullException(nameof(Metadata));
                //if (Metadata.Attribute == null) throw new ArgumentNullException(nameof(Metadata.Attribute));
                //if (Metadata.Property == null) throw new ArgumentNullException(nameof(Metadata.Property));

                _metadataAttribute = Metadata.Attribute;
                
                var propertyInfo = Metadata.PropertyInfo;
                _propertyType = propertyInfo.PropertyType;
                _nullableUnderlyingType = Nullable.GetUnderlyingType(_propertyType);

                CheckIfInputNumber();
                InitCultureAndFormat();

                // must redefine the field identifier alternatively
                FieldIdentifier = new FieldIdentifier(Metadata.Model, propertyInfo.Name);

                // for invoking the method SetCurrentValue(object value) when the value changes
                Metadata.Attach(this);
            }
        }

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var elementName = GetElement(out var elementType);
            var sequence = 0;

            if (elementType == "radio")
            {
                sequence = RenderRadioOptions(builder, sequence);
            }
            else if (_propertyType.SupportsCheckbox(elementType))
            {
                sequence = RenderFormCheck(builder, sequence, radio: false, label: Metadata.GetDisplayName());
            }
            else if (elementType == "file")
            {
                sequence = RenderInputFile(builder, sequence);
            }
            else
            {
                builder.OpenElement(sequence++, elementName);
                builder.AddMultipleAttributes(sequence++, AdditionalAttributes);

                if (elementType != null && elementName == "input")
                {
                    if (elementType == "number")
                        builder.AddAttribute(sequence++, "step", _stepAttributeValue);
                    builder.AddAttribute(sequence++, "type", elementType);
                }

                builder.AddAttribute(sequence++, "class", CssClass);
                
                if (_propertyType.IsString())
                    builder.AddAttribute(sequence++, "value", BindConverter.FormatValue(CurrentValue));
                else
                    builder.AddAttribute(sequence++, "value", BindConverter.FormatValue(CurrentValueAsString));
                
                builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.CreateBinder<string>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
                
                if (elementName == "select")
                    sequence = RenderSelectOptions(builder, sequence);

                builder.CloseElement();
            }
        }

        /// <inheritdoc />
        protected override string FormatValueAsString(object value)
        {
            switch (value)
            {
                case DateTime dateTimeValue:
                    return BindConverter.FormatValue(dateTimeValue, _format, _culture);
                case DateTimeOffset dateTimeOffsetValue:
                    return BindConverter.FormatValue(dateTimeOffsetValue, _format, _culture);
                default:
                    return value?.ToString();
            }
        }

        /// <summary>
        /// Determines the frame to generate representing an HTML element.
        /// </summary>
        /// <param name="elementType">Returns the type of element to generate.</param>
        /// <returns></returns>
        protected virtual string GetElement(out string elementType)
        {
            string element;

            elementType = string.IsNullOrWhiteSpace(_metadataAttribute.UITypeHint) 
                ? null
                : _metadataAttribute.UITypeHint;

            if (!string.IsNullOrWhiteSpace(_metadataAttribute.UIHint))
                element = _metadataAttribute.UIHint;
            else
                element = "input";

            if (elementType == null)
            {
                if (_propertyType == typeof(bool))
                    elementType = "checkbox";
                else if (SupportsInputNumber())
                    elementType = "number";
                else if (SupportsInputDate())
                    elementType = "date";
            }

            return element;
        }

        /// <summary>
        /// Renders option elements for a select element to the supplied <see cref="RenderTreeBuilder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="RenderTreeBuilder"/> that will receive the render output.</param>
        /// <param name="sequence">An integer that represents the position of the instruction in the source code.</param>
        /// <returns>An integer that represents the next position of the instruction in the source code.</returns>
        protected virtual int RenderSelectOptions(RenderTreeBuilder builder, int sequence)
        {
            var propertyInfo = Metadata.PropertyInfo;
            var options = (Form?.OptionsGetter?.Invoke(propertyInfo.Name) ?? Metadata.Options)?.ToList();

            if (options?.Count > 0)
            {
                var promptId = string.Empty;
                var prompt = _metadataAttribute.Prompt;
                var defaultOption = options.Where(opt => opt.IsPrompt).FirstOrDefault();

                if (defaultOption != null)
                {
                    prompt = defaultOption.Value;
                    promptId = defaultOption.Id;

                    options.Remove(defaultOption);
                }

                if (string.IsNullOrWhiteSpace(promptId) && (_nullableUnderlyingType ?? _propertyType).IsNumeric())
                    promptId = "0";

                builder.OpenElement(sequence++, "option");
                builder.AddAttribute(sequence++, "value", promptId);
                builder.AddContent(sequence++, prompt);
                builder.CloseElement();

                foreach (var item in options)
                {
                    builder.OpenElement(sequence++, "option");
                    builder.AddAttribute(sequence++, "value", item.Id);
                    builder.AddContent(sequence++, item.Value);
                    builder.CloseElement();
                }
            }
            else
            {
                builder.AddContent(sequence++, ChildContent);
            }

            return sequence;
        }

        /// <summary>
        /// Renders a collection of input elements of type 'radio' to the supplied <see cref="RenderTreeBuilder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="RenderTreeBuilder"/> that will receive the render output.</param>
        /// <param name="sequence">An integer that represents the position of the instruction in the source code.</param>
        /// <returns>An integer that represents the next position of the instruction in the source code.</returns>
        protected virtual int RenderRadioOptions(RenderTreeBuilder builder, int sequence)
        {
            var propertyName = Metadata.PropertyInfo.Name;
            var options = (Form?.OptionsGetter?.Invoke(propertyName) ?? Metadata.Options)?.ToList();

            if (options?.Count > 0)
            {
                foreach (var item in options)
                {
                    /* This is a variation of what we're building:
                     <div class="form-check">
                        <label class="form-check-label">
                            <input type="radio" class="form-check-input" name="@propertyName" value="@item.Id" @onchange="HandleChange" checked="@IsChecked" />
                            @item.Value
                        </label>
                    </div>
                     */
                    sequence = RenderFormCheck(builder, sequence, true, label: item.Value, name: propertyName, value: item.Id);
                }
            }

            return sequence;
        }

        /// <summary>
        /// Renders a checkbox to the supplied <see cref="RenderTreeBuilder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="RenderTreeBuilder"/> that will receive the render output.</param>
        /// <param name="sequence">An integer that represents the position of the instruction in the source code.</param>
        /// <returns>An integer that represents the next position of the instruction in the source code.</returns>
        protected virtual int RenderInputCheckbox(RenderTreeBuilder builder, int sequence)
        {
            builder.OpenElement(sequence++, "input");
            builder.AddMultipleAttributes(sequence++, AdditionalAttributes);
            builder.AddAttribute(sequence++, "type", "checkbox");
            builder.AddAttribute(sequence++, "class", $"form-check-input {CssClass}".Trim());
            builder.AddAttribute(sequence++, "checked", BindConverter.FormatValue((bool)CurrentValue));
            builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.CreateBinder<bool>(this, __value => CurrentValue = __value, (bool)CurrentValue));
            builder.CloseElement();

            return sequence;
        }

        /// <summary>
        /// Renders a checkbox to the supplied <see cref="RenderTreeBuilder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="RenderTreeBuilder"/> that will receive the render output.</param>
        /// <param name="sequence">An integer that represents the position of the instruction in the source code.</param>
        /// <param name="propertyName">The name of the input radio.</param>
        /// <param name="value">The value of the radio input.</param>
        /// <returns>An integer that represents the next position of the instruction in the source code.</returns>
        protected virtual int RenderInputRadio(RenderTreeBuilder builder, int sequence, string propertyName, string value)
        {
            builder.OpenElement(sequence++, "input");

            builder.AddAttribute(sequence++, "type", "radio");
            builder.AddAttribute(sequence++, "class", $"form-check-input {CssClass}".Trim());
            builder.AddAttribute(sequence++, "name", propertyName);
            builder.AddAttribute(sequence++, "value", BindConverter.FormatValue(value));
            builder.AddAttribute(sequence++, "checked", BindConverter.FormatValue(string.Equals(CurrentValueAsString, value)));
            builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.CreateBinder<string>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
            builder.CloseElement();

            return sequence;
        }

        /// <summary>
        /// Renders to the supplied <see cref="RenderTreeBuilder"/> an input of type file.
        /// </summary>
        /// <param name="builder">A <see cref="RenderTreeBuilder"/> that will receive the render output.</param>
        /// <param name="sequence">An integer that represents the position of the instruction in the source code.</param>
        /// <returns>An integer that represents the next position of the instruction in the source code.</returns>
        protected virtual int RenderInputFile(RenderTreeBuilder builder, int sequence)
        {
            /*
            <div class="custom-file">
                <input type="file" class="custom-file-input" id="@id" name="@(Name ?? id)" title="@Text">
                <label class="custom-file-label" for="@id">@Prompt</label>
            </div>
            */
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "custom-file");

            builder.OpenElement(sequence++, "input");
            builder.AddMultipleAttributes(sequence++, AdditionalAttributes);
            builder.AddAttribute(sequence++, "type", "file");
            builder.AddAttribute(sequence++, "class", $"custom-file-input {CssClass}".Trim());

            // For the file to be uploadable it must have a name.
            var propertyName = Metadata.PropertyInfo.Name;

            builder.AddAttribute(sequence++, "id", propertyName);
            builder.AddAttribute(sequence++, "name", propertyName);

            // Also, the 'onchange' event callback shouldn't change the CurrentValueAsString 
            // property nor should that property be used to initialize the input's value.
            // This is because the value of an input of type file cannot be changed programmatically.
            // The browser reserves this right.

            void __handleFileChange(string __value)
            {
                // Before we pass on the value, let's extract just the file name.
                var filename = string.IsNullOrEmpty(__value) ? __value : Path.GetFileName(__value);

                // Let the form manager decide if the OnFieldChanged event callback should be invoked or not.
                Form?.NotifyFieldChanged(new FormFieldChangedEventArgs(filename, FieldIdentifier, isFile: true));
            }

            builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.CreateBinder<string>(this, __handleFileChange, string.Empty));

            builder.CloseElement(); // /> (input)

            builder.OpenElement(sequence++, "label");
            builder.AddAttribute(sequence++, "class", "custom-file-label");
            builder.AddAttribute(sequence++, "for", propertyName);
            builder.AddContent(sequence++, _metadataAttribute.Prompt ?? Metadata.GetDisplayName());
            builder.CloseElement(); // </label>

            builder.CloseElement(); // </div>

            return sequence;
        }

        /// <inheritdoc/>
        protected override bool TryParseValueFromString(string value, out object result, out string validationErrorMessage)
        {
            var targetType = _nullableUnderlyingType ?? _propertyType;

            if (targetType.IsString())
            {
                result = value;
                validationErrorMessage = null;
                return true;
            }

            bool converted;
            object convertedValue;
            var numberStyles = _metadataAttribute.NumberStyles;

            if (converted = targetType.TryParseByte(value, numberStyles, _culture, out var r1)) convertedValue = r1;
            else if (converted = targetType.TryParseSByte(value, numberStyles, _culture, out var r2)) convertedValue = r2;
            else if (converted = targetType.TryParseChar(value, out var r3)) convertedValue = r3;
            else if (converted = targetType.TryParseInt16(value, numberStyles, _culture, out var r4)) convertedValue = r4;
            else if (converted = targetType.TryParseUInt16(value, numberStyles, _culture, out var r5)) convertedValue = r5;
            else if (converted = targetType.TryParseInt32(value, numberStyles, _culture, out var r6)) convertedValue = r6;
            else if (converted = targetType.TryParseUInt32(value, numberStyles, _culture, out var r7)) convertedValue = r7;
            else if (converted = targetType.TryParseInt64(value, numberStyles, _culture, out var r8)) convertedValue = r8;
            else if (converted = targetType.TryParseUInt64(value, numberStyles, _culture, out var r9)) convertedValue = r9;
            else if (converted = targetType.TryParseSingle(value, numberStyles, _culture, out var r10)) convertedValue = r10;
            else if (converted = targetType.TryParseDouble(value, numberStyles, _culture, out var r11)) convertedValue = r11;
            else if (converted = targetType.TryParseDecimal(value, numberStyles, _culture, out var r12)) convertedValue = r12;
            else if (converted = targetType.TryParseBoolean(value, out var r13)) convertedValue = r13;
            else if (converted = targetType.TryParseDateTime(value, _format, _culture, out var r14)) convertedValue = r14;
            else if (converted = targetType.TryParseDateTimeOffset(value, _format, _culture, out var r15)) convertedValue = r15;
            else
            {
                return TryParseValueFromStringUltimately(value, out result, out validationErrorMessage);
            }
            
            if (converted)
            {
                result = convertedValue;
                validationErrorMessage = null;
                return true;
            }
            else
            {
                result = default;
                validationErrorMessage = $"The chosen value ({value}) cannot be converted to type {targetType}.";
                return false;
            }
        }

        /// <summary>
        /// Sets the specified value to the <see cref="InputBase{TValue}.CurrentValue"/> property
        /// and eventually invokes the <see cref="FormManagerBase.NotifyFieldChanged(FormFieldChangedEventArgs)"/>
        /// method.
        /// </summary>
        /// <param name="value">The value that was changed.</param>
        /// <param name="propertyName">The name of the property that was changed.</param>
        protected virtual void SetCurrentValue(object value, string propertyName)
        {
            CurrentValue = value;
            Form?.NotifyFieldChanged(new FormFieldChangedEventArgs(value, FieldIdentifier));
        }

        #region helpers

        void IAutoInputComponent.SetCurrentValue(object value, string propertyName)
        {
            SetCurrentValue(value, propertyName);
        }

        private bool TryParseValueFromStringUltimately(string value, out object result, out string validationErrorMessage)
        {
            try
            {
                if (BindConverter.TryConvertTo<object>(value, _culture, out var parsedValue))
                {
                    result = parsedValue;
                    validationErrorMessage = null;
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
            }
            result = default;
            validationErrorMessage = $"The {FieldIdentifier.FieldName} field is not valid.";
            return false;
        }

        private int RenderFormCheck(RenderTreeBuilder builder, int sequence, bool radio, string label, string name = null, string value = null)
        {
            /*
            <div class="form-check">
                <label class="form-check-label" title="@attr.Description">
                    <InputCheckbox @bind-Value="CheckboxValue" class="form-check-input" /> @labelText
                </label>
            </div>
             */

            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "form-check");

            builder.OpenElement(sequence++, "label");
            builder.AddAttribute(sequence++, "class", "form-check-label");

            if (radio) sequence = RenderInputRadio(builder, sequence, name, value);
            else sequence = RenderInputCheckbox(builder, sequence);

            builder.AddContent(sequence++, label);

            builder.CloseElement(); // </label>
            builder.CloseElement(); // </div>

            return sequence;
        }

        private void CheckIfInputNumber()
        {
            GetElement(out var elementType);

            if (elementType == "number")
            {
                var targetType = _nullableUnderlyingType ?? _propertyType;
                if (targetType.SupportsInputNumber())
                    _stepAttributeValue = "any";
                else
                    throw new InvalidOperationException($"The type '{targetType}' is not a supported numeric type.");
            }
        }

        private bool SupportsInputNumber() => (_nullableUnderlyingType ?? _propertyType).SupportsInputNumber();

        private bool SupportsInputDate() => (_nullableUnderlyingType ?? _propertyType).IsDate();

        private void InitCultureAndFormat()
        {
            if (!string.IsNullOrWhiteSpace(_metadataAttribute.CultureName))
                _culture = CultureInfo.GetCultureInfo(_metadataAttribute.CultureName);
            else
                _culture = CultureInfo.CurrentCulture;

            if (!string.IsNullOrWhiteSpace(_metadataAttribute.Format))
                _format = _metadataAttribute.Format;
            else if (SupportsInputDate())
                _format = "yyyy-MM-dd"; // Compatible with HTML date inputs
        }

        #endregion
    }
}
