using Carfamsoft.Model2View.Annotations;
using Carfamsoft.Model2View.Shared;
using Carfamsoft.Model2View.Shared.Extensions;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
#else
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
#endif
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static Carfamsoft.Model2View.Mvc.RazorViewConfig;
using Carfamsoft.Model2View.Mvc.Models;

namespace Carfamsoft.Model2View.Mvc
{
    /// <summary>
    /// Provides extension methods for HTML helpers.
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Renders the specified model as an HTML-encoded string that should not be encoded again.
        /// </summary>
        /// <param name="_">The unused HtmlHelper.</param>
        /// <param name="model">The model to render.</param>
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
        /// <returns>An HTML-encoded string that should not be encoded again.</returns>
        public static
#if NETSTANDARD2_0
            IHtmlContent
#else
            IHtmlString
#endif
            RenderAsHtml(
#if NETSTANDARD2_0
            this IHtmlHelper _,
#else
            this HtmlHelper _,
#endif
            object model,
            string jsClientModelPrefix = null,
            string jsClientAttributeName = "ng-model")
        {
            return new HtmlString(new NestedTagBuilder("div").RenderAsHtmlString(
                model, jsClientModelPrefix, jsClientAttributeName));
        }

        /// <summary>
        /// Renders the specified model as an HTML form-encoded string that should not be encoded again.
        /// </summary>
        /// <param name="helper">The used HtmlHelper.</param>
        /// <param name="model">The model to render.</param>
        /// <param name="ngModel">
        /// The model prefix for the AngularJS ng-model attribute.
        /// If the value is null the ng-mnodel attribute is not added.
        /// If the value is empty the attribute is added without the prefix.
        /// </param>
        /// <param name="formName">The name of the form to use. If not specified, a randomly-generated name will be used.</param>
        /// <param name="ngSubmitFunctionName">The name of the AngularJS function used to submit the form.</param>
        /// <param name="submitText">The text of the submit button, if included.</param>
        /// <param name="submitUrl">The URL of the form action. Is ignored if <paramref name="ngModel"/> is specified.</param>
        /// <param name="omitSubmitButton">true to omit the submit button, otherwise, false.</param>
        /// <param name="additionalFormAttributes">Additional form attributes to include.</param>
        /// <param name="additionalHtml">Additional HTML content to include into the form.</param>
        /// <param name="generateNameAttributes">true to generate the 'name' atribute for rendrered form controls.</param>
        /// <returns>An HTML-encoded string that should not be encoded again.</returns>
        public static
#if NETSTANDARD2_0
            IHtmlContent
#else
            IHtmlString
#endif
            AutoEditForm(
#if NETSTANDARD2_0
            this IHtmlHelper helper,
#else
            this HtmlHelper helper,
#endif
            object model,
            string ngModel = null,
            string formName = null,
            string ngSubmitFunctionName = "submitForm()",
            string submitText = "Submit",
            string submitUrl = null,
            bool omitSubmitButton = true,
            IDictionary<string, string> additionalFormAttributes = null,
#if NETSTANDARD2_0
            IHtmlContent
#else
            IHtmlString
#endif 
            additionalHtml = null,
            bool generateNameAttributes = false)
        {
            var form = new NestedTagBuilder("form");
            var attributes = form.Attributes;

            if (ngModel.IsNotBlank())
            {
                if (formName.IsBlank())
                    formName = $"ngform_{Guid.NewGuid().GetHashCode():x}";

                attributes.Add("name", formName);

                if (ngSubmitFunctionName.IsNotBlank())
                    attributes.Add("ng-submit", $"{formName}.$valid && {ngSubmitFunctionName}");

                attributes.Add("novalidate", "novalidate");
            }
            else if (submitUrl.IsNotBlank())
            {
                attributes.Add("action", submitUrl);
                attributes.Add("method", "post");
                attributes.Add("enctype", "multipart/form-data");
            }

            if (additionalFormAttributes?.Count > 0)
            {
                foreach (var kvp in additionalFormAttributes)
                    attributes[kvp.Key] = kvp.Value;
            }

            var options = ControlRenderOptions.CreateDefault();
            options.GenerateNameAttribute = generateNameAttributes;

            form.Render(model, ngModel, renderOptions: options);

            if (additionalHtml != null)
            {
                var html = additionalHtml.ToHtmlString();
                form.AddChild(new NestedTagBuilder("fieldset").AddContent(html));
            }

            if (!omitSubmitButton)
            {
                form.AddChild(new NestedTagBuilder("button")
                    .AddAttribute("type", "submit")
                    .AddClass("btn btn-primary")
                    .AddContent(submitText));
            }

            var token = helper.AntiForgeryToken().ToHtmlString();
            form.AddChild(new NestedTagBuilder("fieldset").AddContent(token));

            return new HtmlString(form.ToString());
        }

        /// <summary>
        /// Renders the specified model as an HTML-encoded string that should not be encoded again.
        /// </summary>
        /// <param name="_">Unused</param>
        /// <param name="viewModel">The view model to render.</param>
        /// <param name="formAttrs">An object that encapsulates form attribute values.</param>
        /// <param name="renderOptions">An object that controls a part of the HTML generation process.</param>
        /// <returns>An HTML-encoded string that should not be encoded again.</returns>
        public static
#if NETSTANDARD2_0
            IHtmlContent
#else
            IHtmlString
#endif 
            AutoEditForm(
#if NETSTANDARD2_0
            this IHtmlHelper _,
#else
            this HtmlHelper _,
#endif
            object viewModel,
            FormAttributes formAttrs = null,
            ControlRenderOptions renderOptions = null)
        {
            return new HtmlString(
                new NestedTagBuilder("div")
                    .RenderAutoEditForm(formAttrs, viewModel, renderOptions)
                    .GetInnerHtml());
        }

#region Partial View Renderer Extensions

        /// <summary>
        /// Automatically renders the specified model using a custom partial view named 
        /// after the value of the <see cref="FormDisplayAutoEdit"/> property.
        /// </summary>
        /// <param name="html">The HtmlHelper used to render the view.</param>
        /// <param name="viewModel">The model to automatically render.</param>
        /// <param name="renderOptions">The control render options.</param>
        /// <param name="viewModelType">The type of <paramref name="viewModel"/>.</param>
        /// 
        /// <returns></returns>
        /// 
        public static
#if NETSTANDARD2_0
            Task<IHtmlContent> AutoRenderAsync(this IHtmlHelper html,
#else
            IHtmlString AutoRender(this HtmlHelper html,
#endif
            object viewModel,
            ControlRenderOptions renderOptions = null,
            Type viewModelType = null)
        {
            var model = new DisplayAutoEditModel
            {
                ViewModel = viewModel,
                ViewModelType = viewModelType,
                RenderOptions = renderOptions,
            };
#if NETSTANDARD2_0
            return html.PartialAsync(FormDisplayAutoEdit, model);
#else
            return html.Partial(FormDisplayAutoEdit, model);
#endif
        }
#endregion
    }
}
