using Carfamsoft.Model2View.Shared;
using Carfamsoft.Model2View.Shared.Extensions;
using Carfamsoft.Model2View.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Carfamsoft.Model2View.WebPages
{
    /// <summary>
    /// A base class for form input components generated on the fly.
    /// </summary>
    public class AutoInputBase
    {
        #region fields

        private readonly Type _propertyType;
        private readonly FormDisplayAttribute _metadataAttribute;
        private readonly Type _nullableUnderlyingType;
        private string _stepAttributeValue; // Null by default, so only allows whole numbers as per HTML spec
        private CultureInfo _culture;
        private string _format;
        //private readonly string _inputId;
        private readonly ControlRenderOptions _renderOptions;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoInputBase"/> class.
        /// </summary>
        /// <param name="metadata">The metadata for generating a form input.</param>
        /// <param name="viewModel">An object used to obtain the value of the associated property defined in <paramref name="metadata"/>.</param>
        /// <param name="renderOptions">An object that controls a part of the HTML generation process.</param>
        /// <param name="propertyNavigationPath">The navigation path to the property being rendered.</param>
        /// <exception cref="ArgumentNullException"><paramref name="metadata"/> is null.</exception>
        public AutoInputBase(AutoInputMetadata metadata, object viewModel, ControlRenderOptions renderOptions = null, string propertyNavigationPath = null)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            Value = metadata.GetValue(viewModel);

            _renderOptions = renderOptions;
            _metadataAttribute = metadata.Attribute;
            CssClass = _metadataAttribute.InputCssClass;

            _propertyType = metadata.PropertyInfo.PropertyType;
            _nullableUnderlyingType = Nullable.GetUnderlyingType(_propertyType);

            PropertyNavigationPath = propertyNavigationPath;

            CheckIfInputNumber();
            InitCultureAndFormat();
        }

        #region properties

        /// <summary>
        /// Gets or sets metadata for generating a form input.
        /// </summary>
        public AutoInputMetadata Metadata { get; set; }

        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        public string CssClass { get; set; }

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the current value as a string.
        /// </summary>
        public string CurrentValueAsString { get; set; }

        /// <summary>
        /// Gets or sets the navigation path to the property 
        /// that renders the current <see cref="AutoInputBase"/>.
        /// </summary>
        public string PropertyNavigationPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets the input's identifier.
        /// </summary>
        protected string InputId { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the input's name.
        /// </summary>
        protected string InputName { get; private set; } = string.Empty;

        ///// <summary>
        ///// Gets or sets a collection of additional attributes that will be applied to the created element.
        ///// </summary>
        //public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        #endregion

        /// <summary>
        /// Renders the component to the specified <see cref="INestedTagBuilder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="INestedTagBuilder"/> that will receive the render output.</param>
        /// <returns>A reference to <paramref name="builder"/>.</returns>
        public virtual INestedTagBuilder BuildRenderTree(INestedTagBuilder builder)
        {
            var elementName = _metadataAttribute.GetElement(out var elementType);
            (InputId, InputName) = Metadata.GenerateIdAndName(_renderOptions, PropertyNavigationPath);

            if (_metadataAttribute.IsInputRadio)
            {
                RenderRadioOptions(builder);
            }
            else if (_propertyType.SupportsCheckbox(elementType))
            {
                if (_metadataAttribute.CustomRenderMode == CustomRenderMode.Enabled)
                    RenderCustomFormCheck(builder, false, Metadata.GetDisplayName(), InputName, null);
                else
                    RenderInputCheckbox(builder, Metadata.GetDisplayName(), null);
            }
            else if (elementType.EqualsIgnoreCase("file"))
            {
                if (_metadataAttribute.CustomRenderMode == CustomRenderMode.Enabled)
                    RenderCustomInputFile(builder);
                else
                    RenderInputFile(builder, null);
            }
            else
            {
                RenderElement(builder, elementName, elementType);
            }

            return builder;
        }

        /// <summary>
        /// Renders an HTML element to the <paramref name="builder"/> render output.
        /// </summary>
        /// <param name="builder">A <see cref="INestedTagBuilder"/> that will receive the render output.</param>
        /// <param name="elementName">The name of the HTML element to render.</param>
        /// <param name="elementType">The type of the HTML element to render.</param>
        public virtual void RenderElement(INestedTagBuilder builder, string elementName, string elementType)
        {
            var elementBuilder = builder.Create(elementName)
                .AddMultipleAttributes(GetAdditionalAttributes(InputName, elementType));

            if (elementType != null && elementName.EqualsIgnoreCase("input"))
            {
                elementBuilder
                    .AddAttributeIf(elementType.EqualsIgnoreCase("number"), "step", _stepAttributeValue)
                    .AddAttributeIfNotBlank("type", elementType);
            }

            var data = Metadata;
            var attr = data.Attribute;

            elementBuilder
                .AddClass(CssClass)
                .AddAttributeIfNotBlank("id", InputId)
                .AddAttributeIfNotBlank("title", data.GetDisplayString(attr.Description))
                .AddAttribute("placeholder", data.GetDisplayString(attr.Prompt) ?? data.GetDisplayString(attr.Description) ?? data.GetDisplayName())
                .AddAttributeIfNotBlank("name", InputName)
                .AddAttributeIf(data.IsRequired, "required");

            CheckDisabled(elementBuilder);

            var elementValue = BindingConverter.FormatValue(data.PropertyInfo.PropertyType, Value)?.ToString();

            if (elementName.EqualsIgnoreCase("textarea"))
            {
                elementBuilder.SetInnerHtml(elementValue);
            }
            else
            {
                elementBuilder.AddAttributeIfNotBlank("value", elementValue);

                if (elementName.EqualsIgnoreCase("select"))
                    RenderSelectOptions(elementBuilder);
            }

            builder.AddContent(elementBuilder.ToString());
        }

        /// <summary>
        /// Formats the value as a string. Derived classes can override this to determine 
        /// the formating used for <see cref="CurrentValueAsString"/>.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>A string representation of the value.</returns>
        protected virtual string FormatValueAsString(object value)
        {
            switch (value)
            {
                case DateTime dateTimeValue:
                    return BindingConverter.FormatValue(dateTimeValue, _format, _culture);
                case DateTimeOffset dateTimeOffsetValue:
                    return BindingConverter.FormatValue(dateTimeOffsetValue, _format, _culture);
                default:
                    return value?.ToString();
            }
        }

        /// <summary>
        /// Renders option elements for a select element to the supplied <see cref="INestedTagBuilder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="INestedTagBuilder"/> that will receive the render output.</param>
        /// <returns>An integer that represents the next position of the instruction in the source code.</returns>
        public virtual void RenderSelectOptions(INestedTagBuilder builder)
        {
            var propertyInfo = Metadata.PropertyInfo;
            var options = (_renderOptions?.OptionsGetter?.Invoke(propertyInfo) ?? Metadata.Options)?.ToList();

            if (options?.Count > 0)
            {
                var selectedValue = $"{Value}";
                var promptId = string.Empty;
                var prompt = Metadata.GetDisplayString(_metadataAttribute.Prompt);
                var defaultOption = options.Where(opt => opt.IsPrompt).FirstOrDefault();

                if (defaultOption != null)
                {
                    prompt = defaultOption.Value;
                    promptId = defaultOption.Id;

                    options.Remove(defaultOption);
                }

                if (promptId.IsBlank() && (_nullableUnderlyingType ?? _propertyType).IsNumeric())
                    promptId = "0";

                __createOption(promptId, prompt);

                foreach (var item in options)
                {
                    __createOption(item.Id, item.Value);
                }

                void __createOption(string __id, string __value)
                {
                    builder.AddChild(builder.Create("option")
                        .AddAttribute("value", __id)
                        .AddAttributeIf(string.Equals(selectedValue, __id), "selected")
                        .AddContent(__value)
                    );
                }
            }
            else
            {
                //builder.AddContent(ChildContent);
            }
        }

        /// <summary>
        /// Renders a collection of input elements of type 'radio' to the supplied <see cref="INestedTagBuilder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="INestedTagBuilder"/> that will receive the render output.</param>
        /// <returns>An integer that represents the next position of the instruction in the source code.</returns>
        public virtual void RenderRadioOptions(INestedTagBuilder builder)
        {
            var propertyName = Metadata.PropertyInfo.Name;
            var options = _renderOptions?.OptionsGetter?.Invoke(Metadata.PropertyInfo) ?? Metadata.Options;

            if (options?.Any() == true)
            {
                var customRadio = _metadataAttribute.CustomRenderMode == CustomRenderMode.Enabled;
                int index = 1;

                foreach (var item in options)
                {
                    if (customRadio)
                    {
                        RenderCustomFormCheck(
                            /* builder */ builder,
                            /* radio */ true,
                            /* label */ item.Value,
                            /* name */ InputName,
                            /* value */ item.Id,
                            index++);
                    }
                    else
                    {
                        RenderInputRadio(
                            /* builder */ builder,
                            /* propertyName */ InputName,
                            /* value */ item.Id,
                            /* additionalCssClass */ null,
                            /* label */ item.Value,
                            index++);
                    }
                }
            }
        }

        /// <summary>
        /// Renders a checkbox to the supplied <see cref="INestedTagBuilder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="INestedTagBuilder"/> that will receive the render output.</param>
        /// <param name="additionalCssClass">The custom CSS class to add to the existing <see cref="CssClass"/>.</param>
        /// <param name="label">The text of the associated label. Should be null if you already wrapped the input inside a label.</param>
        public virtual void RenderInputCheckbox(INestedTagBuilder builder, string additionalCssClass = null, string label = null)
        {
            bool insideLabel = label.IsNotBlank();
            var labelBuilder = insideLabel ? builder.Create("label") : null;
            var inputBuilder = builder.Create("input")
                .AddMultipleAttributes(GetAdditionalAttributes(InputName, "checkbox"))
                .AddAttribute("type", "checkbox")
                .AddClass($"{additionalCssClass} {CssClass}".Trim())
                .AddAttributeIfNotBlank("id", InputId)
                .AddAttributeIfNotBlank("name", InputName)
                .AddAttributeIf(Metadata.IsRequired, "required");

            CheckDisabled(inputBuilder);

            inputBuilder.AddAttributeIf(((bool?)Value) ?? false, "checked").AddAttribute("value", "true");

            if (insideLabel)
            {
                labelBuilder
                    .AddContent("&nbsp;")
                    .AddContent(label)
                    .AddChild(inputBuilder);

                builder.AddContent(labelBuilder.ToString());
            }
            else
            {
                builder.AddChild(inputBuilder);
            }
        }

        /// <summary>
        /// Renders an input radio to the supplied <see cref="INestedTagBuilder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="INestedTagBuilder"/> that will receive the render output.</param>
        /// <param name="propertyName">The name of the input radio.</param>
        /// <param name="value">The value of the radio input.</param>
        /// <param name="additionalCssClass">The custom CSS class to add to the existing <see cref="CssClass"/>.</param>
        /// <param name="label">The text of the associated label. Should be null if you already wrapped the input inside a label.</param>
        /// <param name="index">A descriminating number to append to the input identifier.</param>
        public virtual void RenderInputRadio(INestedTagBuilder builder, string propertyName, object value, string additionalCssClass = null, string label = null, int? index = null)
        {
            /*
            <label>
                <input type="radio" class="form-check-input" name="@propertyName" value="@item.Id" @onchange="HandleChange" checked="@IsChecked" />
                @item.Value
            </label>
            */
            var insideLabel = label.IsNotBlank();
            var labelBuilder = insideLabel ? builder.Create("label") : null;

            var inputBuilder = builder.Create("input")
                .AddAttribute("type", "radio")
                .AddAttributeIfNotBlank("value", FormatValueAsString(value))
                .AddClass($"{additionalCssClass} {CssClass}".Trim())
                .AddAttributeIf(_renderOptions?.GenerateNameAttribute ?? true, "name", propertyName)
                .AddAttributeIf(Metadata.IsRequired, "required")
                .AddAttributeIfNotBlank("id", GetUniqueInputId(index));

            CheckDisabled(inputBuilder);

            inputBuilder.AddAttributeIf(Equals(Value, value), "checked");

            if (insideLabel)
            {
                builder.AddChild(labelBuilder
                    .AddContent("&nbsp;")
                    .AddContent(label)
                    .AddChild(inputBuilder)
                );
            }
            else
            {
                builder.AddChild(inputBuilder);
            }
        }

        private string GetUniqueInputId(int? index)
        {
            if (InputId.IsNotBlank())
            {
                var uniqueId = InputId;

                if (index.HasValue)
                    uniqueId += $"{index}";

                return uniqueId;
            }
            return null;
        }

        /// <summary>
        /// Renders an input file to the supplied <see cref="INestedTagBuilder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="INestedTagBuilder"/> that will receive the render output.</param>
        /// <param name="additionalCssClass">The custom CSS class to add to the existing <see cref="CssClass"/>.</param>
        public virtual void RenderInputFile(INestedTagBuilder builder, string additionalCssClass = null)
        {
            var inputBuilder = builder.Create("input")
                .AddMultipleAttributes(GetAdditionalAttributes(InputName, "file"))
                .AddClass($"{additionalCssClass} {CssClass}".Trim())
                .AddAttribute("type", "file")
                .AddAttributeIfNotBlank("id", InputId)
                .AddAttributeIf(_renderOptions?.GenerateNameAttribute ?? true, "name", InputName)
                .AddAttributeIf(Metadata.PropertyInfo.PropertyType.IsNonStringEnumerableOf(typeof(string)), "multiple")
                .AddAttributeIf(Metadata.IsRequired, "required");

            if (_metadataAttribute.GetFileAttribute() != null)
                AddInputFileAttributes(inputBuilder, _metadataAttribute.GetFileAttribute());

            builder.AddChild(inputBuilder);
        }

        /// <summary>
        /// Adds attributes to an input file from the custom attribute <see cref="InputFileAttribute"/>.
        /// </summary>
        /// <param name="builder">A <see cref="INestedTagBuilder"/> that will receive the render output.</param>
        /// <param name="fileAttr">The <see cref="InputFileAttribute"/> to check.</param>
        public virtual void AddInputFileAttributes(INestedTagBuilder builder, InputFileAttribute fileAttr)
        {
            if (fileAttr == null) throw new ArgumentNullException(nameof(fileAttr));

            builder
                .AddAttributeIfNotBlank("accept", fileAttr.Accept)
                .AddAttributeIf(fileAttr.Multiple, "multiple");
        }

        /// <summary>
        /// Renders to the supplied <see cref="INestedTagBuilder"/> an input of type file.
        /// </summary>
        /// <param name="builder">A <see cref="INestedTagBuilder"/> that will receive the render output.</param>
        public virtual void RenderCustomInputFile(INestedTagBuilder builder)
        {
            /*
            <div class="custom-file">
                <input type="file" class="custom-file-input" id="@id" name="@(Name ?? id)" title="@Text">
                <label class="custom-file-label" for="@id">@Prompt</label>
            </div>
            */
            var div = builder.Create("div").AddClass("custom-file");

            RenderInputFile(builder: div, additionalCssClass: "custom-file-input");

            builder.AddContent(
                div.AddChild(
                    builder.Create("label").AddClass("custom-file-label")
                    .AddAttributeIfNotBlank("for", InputId)
                    .AddContent(Metadata.GetDisplayString(_metadataAttribute.Prompt) ?? Metadata.GetDisplayName())
                ).ToString()
            );
        }

        /// <summary>
        /// Renders to the specified <see cref="INestedTagBuilder"/> a checkbox 
        /// or a radio input inside a &lt;div class="form-check"> wrapper element.
        /// </summary>
        /// <param name="builder">A <see cref="INestedTagBuilder"/> that will receive the render output.</param>
        /// <param name="radio">true to render a radio input; otherwise, false to render a checkbox input.</param>
        /// <param name="label">The label text.</param>
        /// <param name="name">The name of the input radio.</param>
        /// <param name="value">The value of the radio input.</param>
        /// <param name="index">A descriminating number to append to the input identifier.</param>
        public virtual void RenderCustomFormCheck(INestedTagBuilder builder, bool radio, string label, string name = null, string value = null, int? index = null)
        {
            /* This is a variation of what we're building:
            <div class="form-check">
                <input type="radio" class="form-check-input" name="@propertyName" value="@item.Id" @onchange="HandleChange" checked="@IsChecked" />
                <label class="form-check-label">@item.Value</label>
            </div>
            */
            const string FORM_CHECK_INPUT = "form-check-input";

            var div = builder.Create("div").AddClass("form-check");
            var ntbLabel = builder.Create("label").AddClass("form-check-label")
                .AddAttributeIfNotBlank("for", GetUniqueInputId(index));

            if (radio)
            {
                RenderInputRadio(
                    builder: div,
                    propertyName: name,
                    value,
                    additionalCssClass: FORM_CHECK_INPUT,
                    label: null,
                    index);

                ntbLabel.AddChild(builder.Create("span").SetInnerHtml(label));
            }
            else
            {
                RenderInputCheckbox(
                    builder:div,
                    additionalCssClass: FORM_CHECK_INPUT,
                    label: null);

                ntbLabel.AddContent(label);
            }

            builder.AddChild(div.AddChild(ntbLabel));
        }

        /// <summary>
        /// Determines at run-time the disabled state of the current <see cref="AutoInputBase"/>
        /// if the associated property was statically-marked as disabled with the property
        /// <see cref="FormAttributeBase.Disabled"/> set to true.
        /// </summary>
        /// <param name="builder">A <see cref="INestedTagBuilder"/> that will receive the render output.</param>
        public virtual void CheckDisabled(INestedTagBuilder builder)
        {
            if (_metadataAttribute.Disabled)
            {
                // the design-time state is 'disabled', now check the run-time state
                bool disabled = _renderOptions?.DisabledGetter?.Invoke(Metadata.PropertyInfo) ?? true;
                builder.AddAttributeIf(disabled, "disabled");
            }
        }

        #region helpers

        private void CheckIfInputNumber()
        {
            _metadataAttribute.GetElement(out var elementType);

            if (elementType.EqualsIgnoreCase("number"))
            {
                var targetType = _nullableUnderlyingType ?? _propertyType;
                if (targetType.SupportsInputNumber())
                    _stepAttributeValue = "any";
                else
                    throw new InvalidOperationException($"The type '{targetType}' is not a supported numeric type.");
            }
        }

        private void InitCultureAndFormat()
        {
            if (_metadataAttribute.CultureName.IsNotBlank())
                _culture = CultureInfo.GetCultureInfo(_metadataAttribute.CultureName);
            else
                _culture = CultureInfo.CurrentCulture;

            if (_metadataAttribute.Format.IsNotBlank())
                _format = _metadataAttribute.Format;
            else if (SupportsInputDate())
                _format = "yyyy-MM-dd"; // Compatible with HTML date inputs
        }

        private bool SupportsInputDate()
        {
            return (_nullableUnderlyingType ?? _propertyType).IsDate();
        }

        private IEnumerable<KeyValuePair<string, object>> GetAdditionalAttributes
        (string fullPropertyName, string elementType, string elementName = "input")
        {
            if (_renderOptions?.AdditionalAttributesGetter != null)
            {
                return _renderOptions.AdditionalAttributesGetter.Invoke(
                    new HtmlRenderInfo(Metadata.PropertyInfo,
                        _renderOptions.CamelCaseId
                            ? fullPropertyName.AsCamelCase()
                            : fullPropertyName,
                        elementType,
                        elementName)
                    );
            }
            return null;
        }

        #endregion
    }
}