using System.Globalization;
using System.Resources;

namespace Carfamsoft.Model2View.Shared.Extensions
{
    /// <summary>
    /// Provides extension methods for instances of the <see cref="ResourceManager"/> class.
    /// </summary>
    public static class ResourceManagerExtensions
    {
        /// <summary>
        /// Returns a localized string for a property name.
        /// </summary>
        /// <param name="resourceManager">
        /// A resource manager that provides convenient access to culture-specific resources.
        /// </param>
        /// <param name="name">The name of the resource to retrieve.</param>
        /// <param name="culture">
        /// An object that represents the culture for which the resource is localized.
        /// </param>
        /// <returns>
        /// The value of the resource localized for the specified culture, or
        /// <paramref name="name"/> if <paramref name="name"/> cannot be found in a resource set.
        /// </returns>
        public static string GetDisplayString(this ResourceManager resourceManager, string name, CultureInfo culture = null)
        {
            if (name.IsNotBlank())
            {
                bool found = false;
                if (resourceManager != null)
                {
                    try
                    {
                        var result = resourceManager.GetString(name, culture);
                        if (result.IsNotBlank())
                        {
                            name = result;
                            found = true;
                        }
                    }
                    catch
                    {
                    }
                }
                
                if (!found && !name!.Contains(" ")) name = name.AsTitleCaseWords();
            }
            return name;
        }
    }
}
