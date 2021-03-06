﻿// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        private string _inputId;

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
        /// Gets or sets the input identifier.
        /// </summary>
        [Parameter]
        public string Id
        {
            get => _inputId;
            set
            {
                if (!string.Equals(_inputId, value))
                {
                    if (!string.IsNullOrWhiteSpace(_inputId))
                    {
                        // Don't change the input identifier once it has been set.
                        // This avoids errors when trying to access it in the DOM via
                        // JavaScript.
                    }
                    else
                    {
                        _inputId = value;
                    }
                }
            }
        }

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

                _metadataAttribute = Metadata.Attribute;

                var propertyInfo = Metadata.PropertyInfo;
                _propertyType = propertyInfo.PropertyType;
                _nullableUnderlyingType = Nullable.GetUnderlyingType(_propertyType);

                CheckIfInputNumber();
                InitCultureAndFormat();

                // must redefine the field identifier alternatively
                FieldIdentifier = new FieldIdentifier(Metadata.Model, propertyInfo.Name);

                if (string.IsNullOrWhiteSpace(_inputId))
                    _inputId = propertyInfo.Name.GenerateId();

                // for invoking the method SetCurrentValue(object value) when the value changes
                Metadata.Attach(this);
            }
        }

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var elementName = GetElement(out var elementType);
            var sequence = 0;

            if (_metadataAttribute.IsInputRadio)
            {
                sequence = RenderRadioOptions(builder, sequence);
            }
            else if (_propertyType.SupportsCheckbox(elementType))
            {
                if (_metadataAttribute.CustomRenderMode == CustomRenderMode.Enabled)
                    sequence = RenderCustomFormCheck(builder, sequence, radio: false, label: Metadata.GetDisplayName());
                else
                    sequence = RenderInputCheckbox(builder, sequence, label: Metadata.GetDisplayName());
            }
            else if (elementType == "file")
            {
                if (_metadataAttribute.CustomRenderMode == CustomRenderMode.Enabled)
                    sequence = RenderCustomInputFile(builder, sequence);
                else
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
                builder.AddAttribute(sequence++, "id", _inputId);

                sequence = CheckDisabled(builder, sequence);

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

                __createOption(promptId, prompt);

                foreach (var item in options)
                {
                    __createOption(item.Id, item.Value);
                }

                void __createOption(string __id, string __value)
                {
                    builder.OpenElement(sequence++, "option");
                    builder.AddAttribute(sequence++, "value", __id);
                    builder.AddContent(sequence++, __value);
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
                var customRadio = _metadataAttribute.CustomRenderMode == CustomRenderMode.Enabled;
                foreach (var item in options)
                {
                    if (customRadio)
                    {
                        /* This is a variation of what we're building:

                         <div class="form-check">
                            <label class="form-check-label">
                                <input type="radio" class="form-check-input" name="@propertyName" value="@item.Id" @onchange="HandleChange" checked="@IsChecked" />
                                @item.Value
                            </label>
                        </div>
                         */
                        sequence = RenderCustomFormCheck(builder, sequence, true, label: item.Value, name: propertyName, value: item.Id);
                    }
                    else
                    {
                        /*
                         <label>
                            <input type="radio" class="form-check-input" name="@propertyName" value="@item.Id" @onchange="HandleChange" checked="@IsChecked" />
                            @item.Value
                        </label>
                         */
                        sequence = RenderInputRadio(builder, sequence, propertyName, item.Id, label: item.Value);
                    }
                }
            }

            return sequence;
        }

        /// <summary>
        /// Renders a checkbox to the supplied <see cref="RenderTreeBuilder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="RenderTreeBuilder"/> that will receive the render output.</param>
        /// <param name="sequence">An integer that represents the position of the instruction in the source code.</param>
        /// <param name="additionalCssClass">The custom CSS class to add to the existing <see cref="InputBase{TValue}.CssClass"/>.</param>
        /// <param name="label">The text of the associated label. Should be null if you already wrapped the input inside a label.</param>
        /// <returns>An integer that represents the next position of the instruction in the source code.</returns>
        protected virtual int RenderInputCheckbox(RenderTreeBuilder builder, int sequence, string additionalCssClass = null, string label = null)
        {
            bool insideLabel = !string.IsNullOrWhiteSpace(label);
            if (insideLabel)
            {
                builder.OpenElement(sequence++, "label");
            }

            builder.OpenElement(sequence++, "input");
            builder.AddMultipleAttributes(sequence++, AdditionalAttributes);
            builder.AddAttribute(sequence++, "type", "checkbox");
            builder.AddAttribute(sequence++, "class", $"{additionalCssClass} {CssClass}".Trim());
            builder.AddAttribute(sequence++, "id", _inputId);

            sequence = CheckDisabled(builder, sequence);

            builder.AddAttribute(sequence++, "checked", BindConverter.FormatValue((bool)CurrentValue));
            builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.CreateBinder<bool>(this, __value => CurrentValue = __value, (bool)CurrentValue));
            builder.CloseElement();

            if (insideLabel)
            {
                builder.AddContent(sequence++, (MarkupString)"&nbsp;");
                builder.AddContent(sequence++, label);
                builder.CloseElement();
            }

            return sequence;
        }

        /// <summary>
        /// Renders an input radio to the supplied <see cref="RenderTreeBuilder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="RenderTreeBuilder"/> that will receive the render output.</param>
        /// <param name="sequence">An integer that represents the position of the instruction in the source code.</param>
        /// <param name="propertyName">The name of the input radio.</param>
        /// <param name="value">The value of the radio input.</param>
        /// <param name="additionalCssClass">The custom CSS class to add to the existing <see cref="InputBase{TValue}.CssClass"/>.</param>
        /// <param name="label">The text of the associated label. Should be null if you already wrapped the input inside a label.</param>
        /// <returns>An integer that represents the next position of the instruction in the source code.</returns>
        protected virtual int RenderInputRadio(RenderTreeBuilder builder, int sequence, string propertyName, string value, string additionalCssClass = null, string label = null)
        {
            var insideLabel = !string.IsNullOrWhiteSpace(label);
            if (insideLabel)
            {
                builder.OpenElement(sequence++, "label");
            }

            builder.OpenElement(sequence++, "input");

            builder.AddAttribute(sequence++, "type", "radio");
            builder.AddAttribute(sequence++, "class", $"{additionalCssClass} {CssClass}".Trim());
            builder.AddAttribute(sequence++, "name", propertyName);
            builder.AddAttribute(sequence++, "value", BindConverter.FormatValue(value));

            sequence = CheckDisabled(builder, sequence);

            builder.AddAttribute(sequence++, "checked", BindConverter.FormatValue(string.Equals(CurrentValueAsString, value)));
            builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.CreateBinder<string>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
            builder.CloseElement();

            if (insideLabel)
            {
                builder.AddContent(sequence++, (MarkupString)"&nbsp;");
                builder.AddContent(sequence++, label);
                builder.CloseElement();
            }

            return sequence;
        }

        /// <summary>
        /// Renders an input file to the supplied <see cref="RenderTreeBuilder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="RenderTreeBuilder"/> that will receive the render output.</param>
        /// <param name="sequence">An integer that represents the position of the instruction in the source code.</param>
        /// <param name="additionalCssClass">The custom CSS class to add to the existing <see cref="InputBase{TValue}.CssClass"/>.</param>
        /// <returns>An integer that represents the next position of the instruction in the source code.</returns>
        protected virtual int RenderInputFile(RenderTreeBuilder builder, int sequence, string additionalCssClass = null)
        {
            builder.OpenElement(sequence++, "input");
            builder.AddMultipleAttributes(sequence++, AdditionalAttributes);
            builder.AddAttribute(sequence++, "type", "file");
            builder.AddAttribute(sequence++, "class", $"{additionalCssClass} {CssClass}".Trim());
            builder.AddAttribute(sequence++, "id", _inputId);

            // For the file to be uploadable it must have a name.
            builder.AddAttribute(sequence++, "name", Metadata.PropertyInfo.Name);

            // Also, the 'onchange' event callback shouldn't change the CurrentValueAsString 
            // property nor should that property be used to initialize the input's value.
            // This is because the value of an input of type file cannot be changed programmatically.
            // The browser reserves this right.

            void __handleFileChange(string __value)
            {
                // Before we pass on the value, let's extract just the file name.
                var filename = string.IsNullOrEmpty(__value) ? __value : Path.GetFileName(__value);
                
                NotifyFieldChanged(filename, isFile: true);
            }

            builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.CreateBinder<string>(this, __handleFileChange, string.Empty));

            if (_metadataAttribute.FileAttribute != null)
                sequence = AddInputFileAttributes(builder, sequence, _metadataAttribute.FileAttribute);

            builder.CloseElement(); // /> (input)
            
            return sequence;
        }

        /// <summary>
        /// Adds attributes to an input file from the custom attribute <see cref="InputFileAttribute"/>.
        /// If the <see cref="FileCapableAttributeBase.Method"/> value of the given <paramref name="fileAttr"/>
        /// is different from <see cref="FileReaderMethod.None"/>, an attempt to register the current
        /// input with the <see cref="Form"/> is made.
        /// </summary>
        /// <param name="builder">A <see cref="RenderTreeBuilder"/> that will receive the render output.</param>
        /// <param name="sequence">An integer that represents the position of the instruction in the source code.</param>
        /// <param name="fileAttr">The <see cref="InputFileAttribute"/> to check.</param>
        /// <returns>An integer that represents the next position of the instruction in the source code.</returns>
        protected virtual int AddInputFileAttributes(RenderTreeBuilder builder, int sequence, InputFileAttribute fileAttr)
        {
            if (fileAttr == null) throw new ArgumentNullException(nameof(fileAttr));

            if (!string.IsNullOrWhiteSpace(fileAttr.Accept))
                builder.AddAttribute(sequence++, "accept", fileAttr.Accept);

            if (fileAttr.Multiple)
                builder.AddAttribute(sequence++, "multiple", BindConverter.FormatValue(fileAttr.Multiple));

            return sequence;
        }

        /// <summary>
        /// Renders to the supplied <see cref="RenderTreeBuilder"/> an input of type file.
        /// </summary>
        /// <param name="builder">A <see cref="RenderTreeBuilder"/> that will receive the render output.</param>
        /// <param name="sequence">An integer that represents the position of the instruction in the source code.</param>
        /// <returns>An integer that represents the next position of the instruction in the source code.</returns>
        protected virtual int RenderCustomInputFile(RenderTreeBuilder builder, int sequence)
        {
            /*
            <div class="custom-file">
                <input type="file" class="custom-file-input" id="@id" name="@(Name ?? id)" title="@Text">
                <label class="custom-file-label" for="@id">@Prompt</label>
            </div>
            */
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "custom-file");

            sequence = RenderInputFile(builder, sequence, "custom-file-input");

            builder.OpenElement(sequence++, "label");
            builder.AddAttribute(sequence++, "class", "custom-file-label");
            builder.AddAttribute(sequence++, "for", _inputId);
            builder.AddContent(sequence++, _metadataAttribute.Prompt ?? Metadata.GetDisplayName());
            builder.CloseElement(); // </label>

            builder.CloseElement(); // </div>

            return sequence;
        }

        /// <summary>
        /// Renders to the specified <see cref="RenderBatchBuilder"/> a checkbox 
        /// or a radio input inside a &lt;div class="form-check"> wrapper element.
        /// </summary>
        /// <param name="builder">A <see cref="RenderTreeBuilder"/> that will receive the render output.</param>
        /// <param name="sequence">An integer that represents the position of the instruction in the source code.</param>
        /// <param name="radio">true to render a radio input; otherwise, false to render a checkbox input.</param>
        /// <param name="label">The label text.</param>
        /// <param name="name">The name of the input radio.</param>
        /// <param name="value">The value of the radio input.</param>
        /// <returns>An integer that represents the next position of the instruction in the source code.</returns>
        protected virtual int RenderCustomFormCheck(RenderTreeBuilder builder, int sequence, bool radio, string label, string name = null, string value = null)
        {
            /*
            <div class="form-check">
                <label class="form-check-label" title="@attr.Description">
                    <InputCheckbox @bind-Value="CheckboxValue" class="form-check-input" /> @labelText
                </label>
            </div>
             */
            const string FORM_CHECK_INPUT = "form-check-input";

            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "form-check");

            builder.OpenElement(sequence++, "label");
            builder.AddAttribute(sequence++, "class", "form-check-label");

            if (radio) sequence = RenderInputRadio(builder, sequence, name, value, FORM_CHECK_INPUT);
            else sequence = RenderInputCheckbox(builder, sequence, FORM_CHECK_INPUT);

            builder.AddContent(sequence++, label);

            builder.CloseElement(); // </label>
            builder.CloseElement(); // </div>

            return sequence;
        }

        /// <summary>
        /// Determines at run-time the disabled state of the current <see cref="AutoInputBase"/>
        /// if the associated property was statically-marked as disabled with the property
        /// <see cref="FormAttributeBase.Disabled"/> set to true.
        /// </summary>
        /// <param name="builder">A <see cref="RenderTreeBuilder"/> that will receive the render output.</param>
        /// <param name="sequence">An integer that represents the position of the instruction in the source code.</param>
        /// <returns>An integer that represents the next position of the instruction in the source code.</returns>
        protected virtual int CheckDisabled(RenderTreeBuilder builder, int sequence)
        {
            if (_metadataAttribute.Disabled)
            {
                bool? state = Form?.DisabledGetter?.Invoke(_propertyType.Name);
                if (!state.HasValue) state = true;

                builder.AddAttribute(sequence++, "disabled", BindConverter.FormatValue(state.Value));
            }
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
            NotifyFieldChanged(value, false);
        }

        /// <summary>
        /// Notifies the parent <see cref="FormManagerBase"/> that a field's value has been changed.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="isFile">true if it was file; otherwise, false.</param>
        protected virtual void NotifyFieldChanged(object value, bool isFile)
        {
            if (Form != null)
            {
                var targetEventArgs = new FormFieldChangedEventArgs(value, 
                    FieldIdentifier, 
                    isFile, 
                    _inputId, 
                    _metadataAttribute.FileAttribute?.Clone()
                );

                // Let the form manager decide if the OnFieldChanged event callback should be invoked or not.
                Form.NotifyFieldChanged(targetEventArgs);
            }
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
