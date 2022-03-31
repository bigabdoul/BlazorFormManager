using System.Collections.Generic;

namespace BlazorFormManager.Components.Web
{
    /// <summary>
    /// Provides extension methods for HTML components' attributes.
    /// </summary>
    public static class HtmlAttributeExtensions
    {
        /// <summary>
        /// Makes sure that a CSS 'class' attribute is returned only if 
        /// <paramref name="value"/> is defined.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns></returns>
        public static IDictionary<string, object> GetCssClass(this string value)
        {
            return Carfamsoft.Model2View.Shared.Collections.CollectionExtensions.GetAttributes(("class", value));
        }
    }
}
