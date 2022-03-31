using Carfamsoft.Model2View.Shared;
using Carfamsoft.Model2View.Shared.Extensions;
using Carfamsoft.Model2View.Annotations;
using Carfamsoft.Model2View.WebPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Carfamsoft.Model2View.Mvc
{
    /// <summary>
    /// Provides extension methods for instances of the <see cref="INestedTagBuilder"/> class.
    /// </summary>
    public static class NestedTagBuilderExtensions
    {
        /// <summary>
        /// Renders the <paramref name="builder"/> and removes the outer tag from the output.
        /// </summary>
        /// <param name="builder">The tag builder from which to remove the outer tag.</param>
        /// <returns></returns>
        public static string GetInnerHtml(this INestedTagBuilder builder)
        {
            if (HtmlTagInfo.IsSelfClosing(builder.TagName))
                // simply returning builder.InnerHtml is not going to render its child tags
                return builder.ToString();

            var sb = new System.Text.StringBuilder(builder.ToString());
            const int BRACKETS = 2; // <>
            var tagLength = builder.TagName.Length + BRACKETS;
            var closingTagLength = tagLength + 1; // the slash like in </div>

            sb.Remove(0, tagLength).Remove(sb.Length - closingTagLength, closingTagLength);
            return sb.ToString();
        }

        /// <summary>
        /// Renders the specified model as an HTML string.
        /// </summary>
        /// <param name="builder">The <see cref="INestedTagBuilder"/> that will contain the render output.</param>
        /// <param name="viewModel">The view model to render.</param>
        /// <param name="jsClientModelPrefix">
        /// The model prefix for the AngularJS ng-model attribute.
        /// If the value is null the ng-mnodel attribute is not added.
        /// If the value is empty the attribute is added without the prefix.
        /// </param>
        /// <param name="jsClientAttributeName">
        /// The JavaScript client attribute name, like 'ng-model' for AngularJS, 
        /// 'v-model' for Vuejs, etc. The default value is 'ng-model'.
        /// </param>
        /// <param name="renderOptions">An object that controls a part of the HTML generation process.</param>
        /// <returns></returns>
        public static string RenderAsHtmlString(
            this INestedTagBuilder builder,
            object viewModel,
            string jsClientModelPrefix = null,
            string jsClientAttributeName = "ng-model",
            ControlRenderOptions renderOptions = null)
        {
            return builder.Render(viewModel, jsClientModelPrefix, jsClientAttributeName, renderOptions).ToString();
        }

        /// <summary>
        /// Renders the specified model to the given <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="INestedTagBuilder"/> that will contain the render output.</param>
        /// <param name="viewModel">The view model to render.</param>
        /// <param name="jsClientModelPrefix">
        /// The model prefix for a JavaScript client attribute (e.g. 'ng-model'
        /// for AngularJS, or 'v-model' for Vuejs).
        /// If the value is null the <paramref name="jsClientAttributeName"/> 
        /// attribute is not added. If the value is empty the attribute is added 
        /// without the prefix.
        /// </param>
        /// <param name="jsClientAttributeName">
        /// The JavaScript client attribute name, like 'ng-model' for AngularJS, 
        /// 'v-model' for Vuejs, etc. The default value is 'ng-model'.
        /// </param>
        /// <param name="renderOptions">An object that controls a part of the HTML generation process.</param>
        /// <returns></returns>
        public static INestedTagBuilder Render(this INestedTagBuilder builder,
            object viewModel,
            string jsClientModelPrefix = null,
            string jsClientAttributeName = "ng-model",
            ControlRenderOptions renderOptions = null)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

            if (renderOptions == null)
                renderOptions = new ControlRenderOptions { CamelCaseId = true };

            if (jsClientModelPrefix != null && renderOptions.AdditionalAttributesGetter == null)
            {
                renderOptions.AdditionalAttributesGetter = info =>
                {
                    var modelName = info.PropertyName;

                    if (info.ElementType.EqualsIgnoreCase("file"))
                        return new Dictionary<string, object> { { "name", modelName } };
                    
                    if (jsClientModelPrefix.Length > 0) 
                        modelName = $"{jsClientModelPrefix}.{modelName}";
                    
                    return new Dictionary<string, object> { { jsClientAttributeName, modelName } };
                };
            }

            if (viewModel.ExtractMetadata(out var groups))
            {
                foreach (var group in groups)
                {
                    builder.RenderFormDisplayGroup(group, viewModel, renderOptions);
                }
            }

            return builder;
        }

        /// <summary>
        /// Renders a 'form' tag using the specified <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="INestedTagBuilder"/> used to create an HTML 'form' tag.</param>
        /// <param name="attrs">An object that contains the form attributes to render.</param>
        /// <returns>
        /// An initialized instance of a class that implements the interface
        /// <see cref="INestedTagBuilder"/>, which represents the rendered form.
        /// </returns>
        public static INestedTagBuilder RenderForm(this INestedTagBuilder builder, FormAttributes attrs = null)
        {
            if (attrs == null) attrs = new FormAttributes();

            var form = builder.Create("form");

            form.AddAttributeIfNotBlank("id", attrs.Id)
                .AddAttributeIfNotBlank("name", attrs.Name)
                .AddAttributeIfNotBlank("action", attrs.Action)
                .AddAttributeIfNotBlank("method", attrs.Method)
                .AddAttributeIfNotBlank("enctype", attrs.EncType);

            foreach (var kvp in attrs.AdditionalAttributes)
                form.MergeAttribute(kvp.Key, kvp.Value);

            return form;
        }

        /// <summary>
        /// Renders the specified model as an HTML string.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="formAttrs">An object that encapsulates form attribute values.</param>
        /// <param name="viewModel">The view model to render.</param>
        /// <param name="renderOptions">A function that supplies additional attributes to the render output.</param>
        /// <returns>An initialized instance of a class that implements <see cref="INestedTagBuilder"/>, representing the form.</returns>
        public static INestedTagBuilder RenderAutoEditForm(
            this INestedTagBuilder builder,
            FormAttributes formAttrs,
            object viewModel,
            ControlRenderOptions renderOptions = null)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

            var form = builder.RenderForm(formAttrs);

            if (renderOptions == null)
                renderOptions = ControlRenderOptions.CreateDefault();

            if (viewModel.ExtractMetadata(out var groups))
            {
                foreach (var group in groups)
                {
                    form.RenderFormDisplayGroup(group, viewModel, renderOptions);
                }
            }

            return form;
        }

        /// <summary>
        /// Renders the specified <paramref name="group"/> into an 
        /// instance of <see cref="INestedTagBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="INestedTagBuilder"/> that will contain the render output.</param>
        /// <param name="group">The grouped collection of <see cref="AutoInputMetadata"/> used to render <paramref name="viewModel"/>.</param>
        /// <param name="viewModel">The view model to render.</param>
        /// <param name="renderOptions">An object that controls a part of an HTML generation process.</param>
        /// <returns>A reference to the given <paramref name="builder"/>.</returns>
        public static INestedTagBuilder RenderFormDisplayGroup(
            this INestedTagBuilder builder,
            FormDisplayGroupMetadata group,
            object viewModel,
            ControlRenderOptions renderOptions = null)
        {
            /* Sample render output (Razor View)
             <fieldset class="display-group">
                @if (group.ShowName)
                {
                    <legend>@group.Name</legend>
                }
                <div class="display-group-body @group.CssClass">
                    @foreach (AutoInputMetadata data in group.Items)
                    {
                        if (data.Attribute.Ignore) continue;
                        <div class="@data.Attribute.ColumnCssClass">
                            <AutoInput Metadata="data">@ChildContent</AutoInput>
                        </div>
                    }
                </div>
            </fieldset>
             */
            return builder.AddChild(
                builder.Create("fieldset").AddClass("display-group")
                    .AddContentIf(group.ShowName, () => builder.Create("legend").AddContent(group.Name).ToString())
                    .AddContent
                    (
                        builder.Create("div").AddClass("display-group-body").AddClass(group.CssClass)
                        .ForEach
                        (
                            group.Items.Where(meta => !meta.Attribute.IsIgnored()),
                            (data, displayGroupBody) =>
                            {
                                displayGroupBody.AddChild(
                                    builder.Create("div")
                                        .AddClass(data.Attribute.ColumnCssClass)
                                        .RenderAutoInput(data, viewModel, renderOptions: renderOptions)
                                );
                            }
                        ).ToString()
                    ));
        }

        /// <summary>
        /// Renders a button to the specified <paramref name="builder"/> children's collection.
        /// </summary>
        /// <param name="builder">The <see cref="INestedTagBuilder"/> that will contain the render output.</param>
        /// <param name="text">The button text.</param>
        /// <param name="type">The type of button to render.</param>
        /// <param name="cssClass">The CSS class to add to the button.</param>
        /// <param name="config">An action used to perform more configuration on the button.</param>
        /// <returns>A reference to <paramref name="builder"/>.</returns>
        public static INestedTagBuilder RenderButton(
            this INestedTagBuilder builder,
            string text = null,
            string type = "submit",
            string cssClass = "btn btn-primary",
            Action<INestedTagBuilder> config = null)
        {
            var button = builder.Create("button");

            builder.AddChild(button);

            config?.Invoke(button);

            button.AddAttribute("type", type)
                .AddClass(cssClass)
                .AddChild(builder.Create("span").AddContent(text));

            return builder;
        }

        internal static INestedTagBuilder RenderAutoInput(
            this INestedTagBuilder builder,
            AutoInputMetadata metadata,
            object viewModel,
            INestedTagBuilder labelContent = null, ControlRenderOptions renderOptions = null)
        {
            var attr = metadata.Attribute;
            var propertyName = metadata.PropertyInfo.Name;

            if (true == renderOptions?.GenerateIdAttribute)
            {
                renderOptions.UniqueId = propertyName.GenerateId(renderOptions.CamelCaseId);
            }

            if (attr.IsInputRadio || metadata.IsInputCheckbox)
            {
                /*
                 <div class="form-group">
                    @if (!string.IsNullOrWhiteSpace(labelText) && attr.IsInputRadio)
                    {
                        <label class="form-label">@labelText</label>
                    }
                    <AutoInputBase Metadata="metadata" @bind-Value="metadata.Value" Id="@inputId" class="@attr.InputCssClass" title="@attr.Description" />
                    <AutoValidationMessage Model="metadata.Model" Property="@propertyName" />
                </div>
                 */
                builder.RenderCheckboxOrRadio(metadata, viewModel, renderOptions);
            }
            else
            {
                /*
                 <InputGroupContainer Id="@inputId" LabelText="@labelText" Icon="@attr.Icon">
                    <ChildContent>
                        <AutoInputBase Metadata="metadata" @bind-Value="metadata.Value" Id="@inputId" class="@attr.InputCssClass" title="@attr.Description" placeholder="@attr.Prompt">
                            @ChildContent
                        </AutoInputBase>
                    </ChildContent>
                    <ValidationContent>
                        <AutoValidationMessage Model="metadata.Model" Property="@propertyName" />
                    </ValidationContent>
                </InputGroupContainer>
                 */
                builder.RenderInputGroup(metadata, viewModel, metadata.GetDisplayName(), labelContent, renderOptions);
            }

            return builder;
        }

        internal static INestedTagBuilder RenderInputGroup(
            this INestedTagBuilder builder,
            AutoInputMetadata metadata,
            object viewModel,
            string labelText = null,
            INestedTagBuilder labelContent = null,
            ControlRenderOptions renderOptions = null)
        {
            /*
             <div class="form-group">
                @if (!string.IsNullOrWhiteSpace(LabelText))
                {
                    <label for="@Id" class="form-label">@LabelText</label>
                }
                else if (LabelContent != null)
                {
                    @LabelContent
                }
                <div class="input-group">
                    @if (!string.IsNullOrWhiteSpace(Icon))
                    {
                        <div class="input-group-prepend">
                            <span class="input-group-text">
                                <i class="@Icon"></i>
                            </span>
                        </div>
                    }
                    @ChildContent
                </div>
                @if (ValidationContent != null)
                {
                    @ValidationContent
                }
            </div>
             */
            return builder.AddChild(builder.Create("div").AddClass("form-group")
                    .AddChildIf(labelText.IsNotBlank(),
                        () => builder.Create("label").AddClass("form-label")
                            .AddAttributeIfNotBlank("for", renderOptions?.UniqueId)
                            .AddContentIfNotBlank(labelText)
                            .AddChildIf(labelContent != null && labelText.IsBlank(), labelContent)
                    ).AddChild(builder.Create("div").AddClass("input-group")
                        .AddContentIf(metadata.Attribute.Icon.IsNotBlank(), () => builder.RenderIcon(metadata.Attribute.Icon))
                        .RenderAutoInputBase(metadata, viewModel, renderOptions)
                    ));
        }

        /// <summary>
        /// Renders the <see cref="AutoInputBase"/> component to the specified <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="INestedTagBuilder"/> that will contain the render output.</param>
        /// <param name="metadata">An object used to render an HTML element.</param>
        /// <param name="viewModel">An object used to obtain the value of the associated property defined in <paramref name="metadata"/>.</param>
        /// <returns>A reference to <paramref name="builder"/>.</returns>
        /// <param name="renderOptions">An object that controls a part of the HTML generation process.</param>
        /// <param name="propertyNavigationPath">The navigation path to the property being rendered.</param>
        public static INestedTagBuilder RenderAutoInputBase(this INestedTagBuilder builder,
            AutoInputMetadata metadata,
            object viewModel, 
            ControlRenderOptions renderOptions = null,
            string propertyNavigationPath = null)
        {
            return new AutoInputBase(metadata, viewModel, renderOptions, propertyNavigationPath).BuildRenderTree(builder);
        }

        internal static string RenderIcon(
            this INestedTagBuilder builder,
            string icon,
            string inputGroupPrependCssClass = "input-group-prepend",
            string inputGroupTextCssClass = "input-group-text")
        {
            return builder.Create("div").AddClass(inputGroupPrependCssClass)
                .AddChild
                (
                    builder.Create("span")
                        .AddClass(inputGroupTextCssClass)
                        .AddChild(builder.Create("i").AddClass(icon))
                ).ToString();
        }

        internal static INestedTagBuilder RenderCheckboxOrRadio(
            this INestedTagBuilder builder,
            AutoInputMetadata metadata,
            object viewModel, ControlRenderOptions renderOptions = null)
        {
            /*
             <div class="form-group">
                @if (!string.IsNullOrWhiteSpace(labelText) && attr.IsInputRadio)
                {
                    <label class="form-label">@labelText</label>
                }
                <AutoInputBase Metadata="metadata" @bind-Value="metadata.Value" Id="@inputId" class="@attr.InputCssClass" title="@attr.Description" />
                <AutoValidationMessage Model="metadata.Model" Property="@propertyName" />
            </div>
             */
            var labelText = metadata.GetDisplayName();
            return builder.AddChild(builder.Create("div").AddClass("form-group")
                .AddChildIf(labelText.IsNotBlank() && metadata.Attribute.IsInputRadio,
                    () => builder.Create("label").AddClass("form-label").AddContent(labelText)
                ).RenderAutoInputBase(metadata, viewModel, renderOptions)
            );
        }
    }
}
