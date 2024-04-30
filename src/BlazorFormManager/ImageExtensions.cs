using BlazorFormManager.Components.Forms;
using Carfamsoft.Model2View.Shared.Extensions;
using System.Collections.Generic;

namespace BlazorFormManager
{
    internal static class ImageExtensions
    {
        internal static bool TryGetImageUrl(this FormManagerBase? form, string propertyName, out string? value, IDictionary<string, object?>? imageAttributes, bool setOnErrorHandler = true)
        {
            value = null;

            if (form?.HasImageRequestedDelegate == true)
            {
                object? obj = null;
                _ = imageAttributes?.TryGetValue(propertyName, out obj);
                var actual = obj?.ToString();
                var src = form.RequestImage(propertyName, actual);
                value = GetFullSource(src);
                if (setOnErrorHandler && value != null && value != actual && imageAttributes != null)
                {
                    // set the original image as fallback when the requested doesn't exist
                    var originalSource = GetFullSource(actual);

                    // 'this.onerror = undefined' avoids infinite recursive 
                    // errors should the original image also be missing
                    imageAttributes["onerror"] = $"javascript: this.onerror = undefined; this.src='{originalSource}'";
                    imageAttributes["data-src"] = originalSource;
                }
            }
            else if (true == imageAttributes?.TryGetValue(propertyName, out var obj))
            {
                value = GetFullSource(obj?.ToString());
            }

            return value != null;

            string? GetFullSource(string? src) => src.IsBlank() ? null : $"{form?.ImageBaseUri?.TrimEnd('/')}/{src}";
        }
    }
}
