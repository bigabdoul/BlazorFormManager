using Carfamsoft.Model2View.Annotations;
using Carfamsoft.Model2View.Shared;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Html;
#else
using System.Web;
#endif

namespace Carfamsoft.Model2View.Mvc
{
    /// <summary>
    /// Provides extension methods for instances of the <see cref="AutoInputMetadata"/> class.
    /// </summary>
    public static class AutoInputMetadataExtensions
    {
        /// <summary>
        /// Renders the specified <paramref name="metadata"/> to the render output.
        /// </summary>
        /// <param name="metadata">The metadata to render.</param>
        /// <param name="viewModel">
        /// An object used to obtain the value of the associated 
        /// property defined in <paramref name="metadata"/>.
        /// </param>
        /// <returns>An HTML-encoded string that should not be encoded again.</returns>
        /// <param name="options">An object that controls a part of the HTML generation process.</param>
        /// <param name="propertyNavigationPath">The navigation path to the property being rendered.</param>
        public static
#if NETSTANDARD2_0
            IHtmlContent
#else
            IHtmlString
#endif
            Render(this AutoInputMetadata metadata, object viewModel, ControlRenderOptions options = null, string propertyNavigationPath = null)
        {
            var output = new NestedTagBuilder("div").RenderAutoInputBase(metadata, viewModel, options, propertyNavigationPath);
            return new HtmlString(output.GetInnerHtml());
        }
    }
}
