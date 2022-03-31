#if NETSTANDARD2_0
using Microsoft.AspNetCore.Html;
using System.Text.Encodings.Web;

namespace Carfamsoft.Model2View.Mvc
{
    /// <summary>
    /// Provides extension methods for instances of classes that implement <see cref="IHtmlContent"/>.
    /// </summary>
    public static class HtmlContentExtensions
    {
        /// <summary>
        /// Gets the content by encoding it with the specified <paramref name="encoder"/>.
        /// </summary>
        /// <param name="html">The HTML content to retrieve.</param>
        /// <param name="encoder">The <see cref="HtmlEncoder"/> which encodes the 
        /// content to be written. If null, the value of <see cref="HtmlEncoder.Default"/> is used.
        /// </param>
        /// <returns></returns>
        public static string ToHtmlString(this IHtmlContent html, HtmlEncoder encoder = null)
        {
            using (var writer = new System.IO.StringWriter())
            {
                html.WriteTo(writer, encoder ?? HtmlEncoder.Default);
                return writer.ToString();
            }
        }
    }
}
#endif