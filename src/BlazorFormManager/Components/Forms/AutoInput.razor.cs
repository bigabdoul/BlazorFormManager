// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Carfamsoft.Model2View.Annotations;
using Carfamsoft.Model2View.Shared;
using Carfamsoft.Model2View.Shared.Collections;
using Carfamsoft.Model2View.Shared.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFormManager.Components.Forms
{
    /// <summary>
    /// A base class for form input components generated on the fly.
    /// </summary>
    public class AutoInputBase : InputBase<object?>
    {
        #region fields

        private Type? _propertyType;
        private FormDisplayAttribute? _formDisplayAttribute;
        private bool _previousParsingAttemptFailed;
        private ValidationMessageStore? _parsingValidationMessages;
        private Type? _nullableUnderlyingType;
        private string? _stepAttributeValue; // Null by default, so only allows whole numbers as per HTML spec
        private CultureInfo? _culture;
        private string _format = string.Empty;

        #endregion

        /// <summary>
        /// Gets or sets metadata for generating a form input.
        /// </summary>
        [Parameter] public AutoInputMetadata Metadata { get; set; } = AutoInputMetadata.Empty;

        /// <summary>
        /// Gets or sets the child content to be rendering inside the element.
        /// </summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Gets or sets the input identifier.
        /// </summary>
        [Parameter]
        public string? Id
        {
            get => InputId;
            set
            {
                if (!string.Equals(InputId, value))
                {
                    if (!string.IsNullOrWhiteSpace(InputId))
                    {
                        // Don't change the input identifier once it has been set.
                        // This avoids errors when trying to access it in the DOM via
                        // JavaScript.
                    }
                    else
                    {
                        InputId = value ?? string.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the input's or element's 'name' attribute value.
        /// </summary>
        [Parameter] public string? Name { get => InputName; set => InputName = value ?? string.Empty; }

        /// <summary>
        /// Gets or sets the navigation path of the property that renders this <see cref="AutoInputBase"/>.
        /// </summary>
        [Parameter] public string? PropertyNavigationPath { get; set; }

        /// <summary>
        /// Gets the form model for this input.
        /// </summary>
        [Parameter] public object? Model { get; set; }

        /// <summary>
        /// Gets the associated <see cref="FormManagerBase"/>.
        /// </summary>
        [CascadingParameter] protected FormManagerBase? Form { get; set; }

        private ControlRenderOptions? RenderOptions => Form?.RenderOptions;

        /// <summary>
        /// Gets the actual input identifier.
        /// </summary>
        private string InputId { get; set; } = string.Empty;

        /// <summary>
        /// Gets the actual input name.
        /// </summary>
        private string InputName { get; set; } = string.Empty;

        /// <inheritdoc/>
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);

            if (_propertyType == null)
            {
                if (Metadata == null) throw new ArgumentNullException($"{nameof(Metadata)} cannot be null.", (Exception?)null);
                if (Model == null) throw new ArgumentNullException($"{nameof(Model)} cannot be null.", (Exception?)null);

                _formDisplayAttribute = Metadata.Attribute;

                var propertyInfo = Metadata.PropertyInfo;
                _propertyType = propertyInfo.PropertyType;
                _nullableUnderlyingType = Nullable.GetUnderlyingType(_propertyType);

                CheckIfInputNumber();
                InitCultureAndFormat();

                // must redefine the field identifier alternatively
                FieldIdentifier = new FieldIdentifier(Model, propertyInfo.Name);
            }
        }

        /// <inheritdoc />
        protected override void OnParametersSet()
        {
            GenerateIdAndName();
            base.OnParametersSet();
        }

        /// <inheritdoc />
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                Value = Metadata.GetValue(Model);
            }
            base.OnAfterRender(firstRender);
        }

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var elementName = _formDisplayAttribute!.GetElement(out var elementType);
            var sequence = 0;

            if (_formDisplayAttribute!.IsInputRadio)
            {
                sequence = RenderRadioOptions(builder, sequence);
            }
            else if (_propertyType.SupportsCheckbox(elementType))
            {
                if (_formDisplayAttribute!.CustomRenderMode == CustomRenderMode.Enabled)
                    sequence = RenderCustomFormCheck(builder, sequence, radio: false, label: Metadata.GetDisplayName());
                else
                    sequence = RenderInputCheckbox(builder, sequence, label: Metadata.GetDisplayName());
            }
            else if (elementType.EqualsIgnoreCase("file"))
            {
                if (_formDisplayAttribute!.CustomRenderMode == CustomRenderMode.Enabled)
                    sequence = RenderCustomInputFile(builder, sequence);
                else
                    sequence = RenderInputFile(builder, sequence);
            }
            else
            {
                // experimental
                var isRichText = _formDisplayAttribute.RichText || true == Form?.EnableRichText && elementName.EqualsIgnoreCase("textarea");

                if (isRichText)
                {
                    // Quill rich text editor (v1.3.6) doesn't support the textarea
                    // element; so we use a 'div' and later create a hidden input
                    // which stores the input's current value.
                    elementName = "div";
                }

                builder.OpenElement(sequence++, elementName);
                var extraAttributes = _formDisplayAttribute!.GetExtraAttributes();

                builder.AddMultipleAttributes(sequence++, AdditionalAttributes.Merge(extraAttributes));

                if (!isRichText && elementType != null && elementName.EqualsIgnoreCase("input"))
                {
                    if (elementType.EqualsIgnoreCase("number"))
                    {
                        builder.AddAttribute(sequence++, "step", _stepAttributeValue);
                    }
                    builder.AddAttribute(sequence++, "type", elementType);
                }

                builder.AddAttribute(sequence++, "id", InputId);
                
                if (!isRichText && GenerateNameAttribute)
                    builder.AddAttribute(sequence++, "name", InputName);
                
                builder.AddAttribute(sequence++, "class", $"{CssClass} {_formDisplayAttribute.InputCssClass}".Trim());
                
                sequence = CheckDisabled(builder, sequence);
                var isstring = _propertyType.IsString();

                if (!isRichText)
                {
                    // add the 'value' attribute with the current value
                    if (isstring)
                        builder.AddAttribute(sequence++, "value", BindConverter.FormatValue(CurrentValue));
                    else
                        builder.AddAttribute(sequence++, "value", BindConverter.FormatValue(CurrentValueAsString));

                    builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.CreateBinder<string?>(this, 
                        __value => CurrentValueAsString = __value, CurrentValueAsString));
                }

                if (elementName.EqualsIgnoreCase("select"))
                    sequence = RenderSelectOptions(builder, sequence);
                
                if (!isRichText && _formDisplayAttribute.Placeholder.IsNotBlank())
                    builder.AddAttribute(sequence++, "placeholder", _formDisplayAttribute.Placeholder);

                if (!isRichText && Metadata.IsRequired)
                    builder.AddAttribute(sequence++, "required", true);

                if (isRichText)
                {
                    // get the current value as a raw HTML markup string
                    var content = isstring 
                        ? BindConverter.FormatValue(CurrentValue) 
                        : BindConverter.FormatValue(CurrentValueAsString);
                    
                    builder.AddContent(sequence++, new MarkupString($"{content}"));
                }

                builder.CloseElement();

                if (isRichText)
                {
                    builder.OpenElement(sequence++, "input");
                    builder.AddAttribute(sequence++, "type", "hidden");
                    builder.AddAttribute(sequence++, "name", GenerateIdAndName(true).AsCamelCase());
                    builder.AddAttribute(sequence++, "id", InputId + "_hidden");
                    if (isstring)
                        builder.AddAttribute(sequence++, "value", BindConverter.FormatValue(CurrentValue));
                    else
                        builder.AddAttribute(sequence++, "value", BindConverter.FormatValue(CurrentValueAsString));
                    builder.CloseElement();
                }
            }
        }

        /// <inheritdoc />
        protected override string FormatValueAsString(object? value)
        {
            return value switch
            {
                DateTime dateTimeValue => BindConverter.FormatValue(dateTimeValue, _format!, _culture),
                DateTimeOffset dateTimeOffsetValue => BindConverter.FormatValue(dateTimeOffsetValue, _format!, _culture),
                _ => value?.ToString() ?? string.Empty,
            };
        }

        /// <summary>
        /// Renders option elements for a select element to the supplied <see cref="RenderTreeBuilder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="RenderTreeBuilder"/> that will receive the render output.</param>
        /// <param name="sequence">An integer that represents the position of the instruction in the source code.</param>
        /// <returns>An integer that represents the next position of the instruction in the source code.</returns>
        protected virtual int RenderSelectOptions(RenderTreeBuilder builder, int sequence)
        {
            var options = Metadata.GetSelectOptions(Form?.OptionsGetter);

            if (options?.Count > 0)
            {
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
                var customRadio = _formDisplayAttribute!.CustomRenderMode == CustomRenderMode.Enabled;
                foreach (var item in options)
                {
                    if (customRadio)
                    {
                        /* This is a variation of what we're building:

                         <div class="form-check">
                            <input type="radio" class="form-check-input" name="@propertyName" value="@item.Id" @onchange="HandleChange" checked="@IsChecked" />
                            <label class="form-check-label">@item.Value</label>
                        </div>
                         */
                        sequence = RenderCustomFormCheck(builder, sequence, true, label: item.Value, name: propertyName, value: item.Id);
                    }
                    else
                    {
                        /*
                        <input type="radio" class="form-check-input" name="@propertyName" value="@item.Id" @onchange="HandleChange" checked="@IsChecked" />
                         <label>@item.Value</label>
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
        protected virtual int RenderInputCheckbox(RenderTreeBuilder builder, int sequence, string? additionalCssClass = null, string? label = null)
        {
            builder.OpenElement(sequence++, "input");
            builder.AddMultipleAttributes(sequence++, AdditionalAttributes);
            builder.AddAttribute(sequence++, "type", "checkbox");
            builder.AddAttribute(sequence++, "class", $"{additionalCssClass} {CssClass}".Trim());
            
            if (InputId.IsNotBlank())
                builder.AddAttribute(sequence++, "id", InputId);

            sequence = CheckDisabled(builder, sequence);

            builder.AddAttribute(sequence++, "checked", BindConverter.FormatValue((bool?)CurrentValue));
            builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.CreateBinder<bool?>(this, __value => CurrentValue = __value, (bool?)CurrentValue));
            builder.CloseElement();

            if (!_formDisplayAttribute!.CheckNoLabel && !string.IsNullOrWhiteSpace(label))
            {
                builder.OpenElement(sequence++, "label");
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
        protected virtual int RenderInputRadio(RenderTreeBuilder builder, int sequence, string? propertyName, string? value, string? additionalCssClass = null, string? label = null)
        {
            builder.OpenElement(sequence++, "input");
            builder.AddAttribute(sequence++, "type", "radio");
            builder.AddAttribute(sequence++, "class", $"{additionalCssClass} {CssClass}".Trim());
            builder.AddAttribute(sequence++, "name", propertyName);
            builder.AddAttribute(sequence++, "value", BindConverter.FormatValue(value));

            sequence = CheckDisabled(builder, sequence);

            builder.AddAttribute(sequence++, "checked", BindConverter.FormatValue(string.Equals(CurrentValueAsString, value)));
            builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
            builder.CloseElement();

            if (!_formDisplayAttribute!.CheckNoLabel && !string.IsNullOrWhiteSpace(label))
            {
                builder.OpenElement(sequence++, "label");
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
        protected virtual int RenderInputFile(RenderTreeBuilder builder, int sequence, string? additionalCssClass = null)
        {
            builder.OpenElement(sequence++, "input");
            builder.AddMultipleAttributes(sequence++, AdditionalAttributes);
            builder.AddAttribute(sequence++, "type", "file");
            builder.AddAttribute(sequence++, "class", $"{additionalCssClass} {CssClass}".Trim());
            builder.AddAttribute(sequence++, "id", InputId);

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

            if (_formDisplayAttribute!.FileAttribute != null)
                sequence = AddInputFileAttributes(builder, sequence, _formDisplayAttribute.FileAttribute);

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
            builder.AddAttribute(sequence++, "for", InputId);
            builder.AddContent(sequence++, _formDisplayAttribute!.Prompt ?? Metadata.GetDisplayName());
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
        protected virtual int RenderCustomFormCheck(RenderTreeBuilder builder, int sequence, bool radio, string label, string? name = null, string? value = null)
        {
            const string FORM_CHECK_INPUT = "form-check-input";
            /* version 1:
            <div class="form-check">
                <label class="form-check-label" title="@attr.Description">
                    <InputCheckbox @bind-Value="CheckboxValue" class="form-check-input" /> @labelText
                </label>
            </div>

            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "form-check");

            builder.OpenElement(sequence++, "label");
            builder.AddAttribute(sequence++, "class", "form-check-label");

            if (radio) sequence = RenderInputRadio(builder, sequence, name, value, FORM_CHECK_INPUT);
            else sequence = RenderInputCheckbox(builder, sequence, FORM_CHECK_INPUT);

            builder.AddContent(sequence++, label);

            builder.CloseElement(); // </label>
            builder.CloseElement(); // </div>
            */

            /*
            version 2:
            <div class="form-check form-switch">
              <input class="form-check-input" type="checkbox" id="flexSwitchCheckDefault">
              <label class="form-check-label" for="flexSwitchCheckDefault">Default switch checkbox input</label>
            </div>
             */
            FormDisplayAttribute attr = _formDisplayAttribute!;
            builder.OpenElement(sequence++, "div");


            if (!attr.CheckNoLabel)
            {
                var classList = $"form-check{(attr.CheckSwitch ? " form-switch" : "")}";
                
                if (attr.CheckInline)
                    classList += $" form-check-inline";
            
                var extraClassList = string.Empty;
                var extraAttrs = attr.GetExtraAttributes();

                if (extraAttrs.ContainsKey("class"))
                    extraClassList = $"{extraAttrs["class"]}";

                builder.AddAttribute(sequence++, "class", $"{classList} {extraClassList}".Trim());
            }

            if (radio)
                sequence = RenderInputRadio(builder, sequence, name, value, FORM_CHECK_INPUT);
            else
                sequence = RenderInputCheckbox(builder, sequence, FORM_CHECK_INPUT);
            
            if (!attr.CheckNoLabel)
            {
                builder.OpenElement(sequence++, "label");
                builder.AddAttribute(sequence++, "class", "form-check-label");
                
                if (InputId.IsNotBlank())
                    builder.AddAttribute(sequence++, "for", InputId);
                
                builder.AddContent(sequence++, label);
                builder.CloseElement();
            }

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
            if (_formDisplayAttribute!.Disabled)
            {
                bool? state = Form?.DisabledGetter?.Invoke(_propertyType?.Name ?? string.Empty);
                if (!state.HasValue) 
                    state = true;
                builder.AddAttribute(sequence++, "disabled", BindConverter.FormatValue(state.Value));
            }
            return sequence;
        }

        /// <inheritdoc/>
        protected override bool TryParseValueFromString
        (
#if NET5_0_OR_GREATER
            string? value, [MaybeNullWhen(false)] out object result, [NotNullWhen(false)] out string? validationErrorMessage
#else
            string? value, out object? result, out string? validationErrorMessage
#endif
        )
        {
            var targetType = _nullableUnderlyingType ?? _propertyType;

            if (targetType.IsString())
            {
                result = value!;
                validationErrorMessage = null;
                return true;
            }

            bool converted;
            object convertedValue;
            var numberStyles = _formDisplayAttribute!.NumberStyles;

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
                result = value;
                validationErrorMessage = $"The chosen value ({value}) cannot be converted to the type {targetType}.";
                return false;
            }
        }

        /// <summary>
        /// Notifies the parent <see cref="FormManagerBase"/> that a field's value has been changed.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="isFile">true if it was file; otherwise, false.</param>
        protected virtual void NotifyFieldChanged(object? value, bool isFile)
        {
            if (Form != null)
            {
                var targetEventArgs = new FormFieldChangedEventArgs(value, 
                    FieldIdentifier, 
                    isFile, 
                    InputId, 
                    _formDisplayAttribute!.FileAttribute?.Clone()
                );

                // Let the form manager decide if the OnFieldChanged event callback should be invoked or not.
                Form.NotifyFieldChanged(targetEventArgs);
            }
        }

#region shadowing

        /// <summary>
        /// Gets or sets the current value of the input.
        /// </summary>
        protected new object? CurrentValue
        {
            get => Metadata?.GetValue(Model);
            set
            {
                var hasChanged = GetNotifyFieldChangedArgs(out var args, CurrentValue, value, Metadata, InputId, FieldIdentifier);
                if (hasChanged)
                {
                    Metadata?.SetValue(Model, value);
                    _ = ValueChanged.InvokeAsync(value);
                    Form?.NotifyFieldChanged(args!);
                }
            }
        }

        /// <summary>
        /// Gets or sets the current value of the input, represented as a string.
        /// </summary>
        protected new string? CurrentValueAsString
        {
            get => FormatValueAsString(CurrentValue);
            set
            {
                _parsingValidationMessages?.Clear();

                bool parsingFailed;

                if (_nullableUnderlyingType != null && string.IsNullOrEmpty(value))
                {
                    // Assume if it's a nullable type, null/empty inputs should correspond to default(T)
                    // Then all subclasses get nullable support almost automatically (they just have to
                    // not reject Nullable<T> based on the type itself).
                    parsingFailed = false;
                    CurrentValue = default!;
                }
                else if (TryParseValueFromString(value, out var parsedValue, out var validationErrorMessage))
                {
                    parsingFailed = false;
                    CurrentValue = parsedValue!;
                }
                else
                {
                    parsingFailed = true;

                    // EditContext may be null if the input is not a child component of EditForm.
                    if (Form?.EditContext is not null && Form.EnableChangeTracking)
                    {
                        _parsingValidationMessages ??= new ValidationMessageStore(Form.EditContext);
                        _parsingValidationMessages.Add(FieldIdentifier, validationErrorMessage);

                        // Since we're not writing to CurrentValue, we'll need to notify about modification from here
                        var changeArgs = new FormFieldChangedEventArgs(value,
                            FieldIdentifier,
                            isFile: Metadata.Attribute.IsInputFile,
                            InputId,
                            Metadata.Attribute.FileAttribute?.Clone()
                        );

                        Form.NotifyFieldChanged(changeArgs);
                    }
                }

                // We can skip the validation notification if we were previously valid and still are
                if (parsingFailed || _previousParsingAttemptFailed)
                {
                    EditContext?.NotifyValidationStateChanged();
                    _previousParsingAttemptFailed = parsingFailed;
                }
            }
        }

        private bool GenerateNameAttribute => true == Form?.GenerateInputNameAttribute || 
        (RenderOptions?.GenerateNameAttribute ?? false);

        #endregion

        #region helpers

        private bool TryParseValueFromStringUltimately(string? value, out object? result, out string validationErrorMessage)
        {
            try
            {
                if (BindConverter.TryConvertTo<object>(value, _culture, out var parsedValue))
                {
                    result = parsedValue;
                    validationErrorMessage = string.Empty;
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
            }
            result = value;
            validationErrorMessage = $"The {FieldIdentifier.FieldName} field is not valid.";
            return false;
        }

        private void CheckIfInputNumber()
        {
            _formDisplayAttribute!.GetElement(out var elementType);

            if (elementType == "number")
            {
                var targetType = _nullableUnderlyingType ?? _propertyType;
                if (targetType.SupportsInputNumber())
                    _stepAttributeValue = "any";
                else
                    throw new InvalidOperationException($"The type '{targetType}' is not a supported numeric type.");
            }
        }

        private bool SupportsInputDate() => (_nullableUnderlyingType ?? _propertyType).IsDate();

        private void InitCultureAndFormat()
        {
            if (!string.IsNullOrWhiteSpace(_formDisplayAttribute!.CultureName))
                _culture = CultureInfo.GetCultureInfo(_formDisplayAttribute!.CultureName);
            else
                _culture = CultureInfo.CurrentCulture;

            if (!string.IsNullOrWhiteSpace(_formDisplayAttribute!.Format))
                _format = _formDisplayAttribute.Format;
            else if (SupportsInputDate())
                _format = "yyyy-MM-dd"; // Compatible with HTML date inputs
        }

        private string GenerateIdAndName(bool? generateInputNameAttribute = null)
        {
            if (InputId.IsBlank() || InputName.IsBlank())
            {
                var (id, name) = Metadata.GenerateIdAndName(RenderOptions, PropertyNavigationPath, Id, generateInputNameAttribute ?? Form?.GenerateInputNameAttribute);

                if (true == Metadata?.Attribute.InputName.IsNotBlank())
                    name = Metadata?.Attribute.InputName!;

                if (InputId.IsBlank())
                    InputId = id;

                if (InputName.IsBlank())
                    InputName = name;
            }

            return InputName;
        }

        internal static bool GetNotifyFieldChangedArgs(out FormFieldChangedEventArgs? result, 
            object? curentValue, object? newValue, AutoInputMetadata? Metadata,
            string? inputId, FieldIdentifier fieldIdentifier)
        {
            if (Metadata != null && !EqualityComparer<object?>.Default.Equals(curentValue, newValue))
            {
                if (Metadata.Attribute.IsInputFile)
                {
                    var svalue = $"{newValue}";
                    newValue = string.IsNullOrEmpty(svalue) ? svalue : Path.GetFileName(svalue);
                }

                result = new FormFieldChangedEventArgs(newValue,
                    fieldIdentifier,
                    isFile: Metadata.Attribute.IsInputFile,
                    inputId,
                    Metadata.Attribute.FileAttribute?.Clone()
                );

                return true;
            }

            result = null;
            return false;
        }

#endregion
    }
}
