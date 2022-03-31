using Carfamsoft.Model2View.Shared;
using Carfamsoft.Model2View.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Carfamsoft.Model2View.Annotations
{
    /// <summary>
    /// Provides extension methods for <see cref="AutoInputMetadata"/> instances.
    /// </summary>
    public static class AutoInputMetadataExtensions
    {
        /// <summary>
        /// Retrieves a collection of options for a select element.
        /// </summary>
        /// <param name="metadata">The metadata related to the element.</param>
        /// <param name="optionsGetter">
        /// A function callback used to retrieve a collection of <see cref="SelectOption"/> items.
        /// </param>
        /// <returns></returns>
        public static IList<SelectOption> GetSelectOptions(this AutoInputMetadata metadata, Func<string, IEnumerable<SelectOption>> optionsGetter = null)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));

            var options = (optionsGetter?.Invoke(metadata.PropertyInfo.Name) ?? metadata.Options)?.ToList();

            if (options?.Count > 0)
            {
                var promptId = string.Empty;
                var prompt = metadata.Attribute.Prompt;
                var defaultOption = options.Where(opt => opt.IsPrompt).FirstOrDefault();

                if (defaultOption != null)
                {
                    prompt = defaultOption.Value;
                    promptId = defaultOption.Id;

                    options.Remove(defaultOption);
                }

                var propertyType = metadata.PropertyInfo.PropertyType;

                if (string.IsNullOrWhiteSpace(promptId) && (Nullable.GetUnderlyingType(propertyType) ?? propertyType).IsNumeric())
                    promptId = "0";

                options.Insert(0, new SelectOption(promptId, prompt, true));

                return options;
            }

            return null;
        }

        /// <summary>
        /// Eventually generates an identifier and name according to the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="metadata">The auto input metadata. Cannot be null.</param>
        /// <param name="options">An object that specifies how to generate the id and name.</param>
        /// <param name="navigationPath">The property navigation path.</param>
        /// <param name="currentId">The current identifier.</param>
        /// <param name="generateNameAttribute">if specified, overrides the <see cref="ControlRenderOptions.GenerateNameAttribute"/> property value.</param>
        /// <returns></returns>
        public static (string id, string name) GenerateIdAndName(this AutoInputMetadata metadata, ControlRenderOptions options = null, string navigationPath = null, string currentId = null, bool? generateNameAttribute = null)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));

            bool camelCase = options?.CamelCaseId ?? false;
            bool isfile = metadata.Attribute.Is("file");
            bool generateId = isfile || (options?.GenerateIdAttribute ?? true);

            if (navigationPath.IsBlank())
                navigationPath = metadata.PropertyInfo.Name;

            string inputId = string.Empty;
            string inputName = string.Empty;

            if (generateId)
            {
                if (string.IsNullOrWhiteSpace(currentId))
                {
                    var appendHash = true;// options?.PredictableId ?? false;
                    inputId = navigationPath.GenerateId(camelCase, appendHash);
                }
                else
                {
                    inputId = currentId;
                }
            }

            if (isfile || generateNameAttribute.HasValue && generateNameAttribute.Value || 
                (options?.GenerateNameAttribute ?? false))
            {
                inputName = camelCase ? navigationPath.AsCamelCase() : navigationPath;
            }

            return (inputId, inputName);
        }
    }
}
