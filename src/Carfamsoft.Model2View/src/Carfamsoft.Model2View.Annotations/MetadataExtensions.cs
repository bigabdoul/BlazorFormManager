using Carfamsoft.Model2View.Shared.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Carfamsoft.Model2View.Annotations
{
    /// <summary>
    /// Provides extension methods for extracting and manipulating metadata, such as custom attributes.
    /// </summary>
    public static class MetadataExtensions
    {
        private static readonly ConcurrentDictionary<string, AttributeBag> _sectionLayoutMetadataCache
            = new ConcurrentDictionary<string, AttributeBag>();

        private const string form_control = "form-control";

        /// <summary>
        /// Generates a grouped collection of <see cref="AutoInputMetadata"/> instances
        /// extracted from custom attributes of types <see cref="FormDisplayAttribute"/>
        /// and <see cref="FormDisplayDefaultAttribute"/> that decorate the specified 
        /// </summary>
        /// <param name="instance">
        /// An initialized instance of an object whose type information will be used to extract metadata.
        /// </param>
        /// <param name="result">
        /// Returns a grouped collection of <see cref="AutoInputMetadata"/> objects.
        /// </param>
        /// <returns></returns>
        public static bool ExtractMetadata(this object instance, out IReadOnlyCollection<FormDisplayGroupMetadata> result)
            => instance.GetType().ExtractMetadata(out result);

        /// <summary>
        /// Generates a grouped collection of <see cref="AutoInputMetadata"/> instances
        /// extracted from custom attributes of types <see cref="FormDisplayAttribute"/>
        /// and <see cref="FormDisplayDefaultAttribute"/> that decorate the specified 
        /// <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of the object from which to extract metadata.</param>
        /// <param name="result">Returns a grouped collection of <see cref="AutoInputMetadata"/> objects.</param>
        /// <returns></returns>
        public static bool ExtractMetadata(this Type type, out IReadOnlyCollection<FormDisplayGroupMetadata> result)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            string fullpath = type.GetNestedTypeHash();

            if (!_sectionLayoutMetadataCache.TryGetValue(fullpath, out var attributeBag))
            {
                attributeBag = new AttributeBag();
                var emptyDisplay = FormDisplayAttribute.Empty;
                var properties = type.GetProperties();
                var ignoreUndecorated = type.GetCustomAttribute<DisplayIgnoreAttribute>() != null;
                var defaultDisplay = type.GetCustomAttributes<FormDisplayDefaultAttribute>().SingleOrDefault() ?? FormDisplayDefaultAttribute.Empty;

                foreach (var pi in properties)
                {
                    // only custom attributes descending from FormAttributeBase are discovered
                    var attributes = pi.GetCustomAttributes<FormAttributeBase>(true).ToList();
                    var hasAttributes = attributes.Any();

                    if (!hasAttributes && ignoreUndecorated) continue;

                    // DisplayIgnoreAttribute doesn't descend from FormAttributeBase;
                    // query the property info directly
                    var isIgnored = pi.GetCustomAttribute<DisplayIgnoreAttribute>() != null;

                    if (isIgnored) continue; // that simple, no questions asked!

                    var inputFileAttr = hasAttributes ? attributes.OfType<InputFileAttribute>().SingleOrDefault() : null;
                    var dragDropAttr = hasAttributes ? attributes.OfType<DragDropAttribute>().SingleOrDefault() : null;

                    if (!(pi.CanRead && pi.CanWrite))
                    {
                        // check if the property is drag and drop enabled
                        // or an input file (no empty value is programmatically set on input file)
                        if (inputFileAttr == null && dragDropAttr == null)
                            continue; // properties must be read-write
                    }

                    var imageAttr = hasAttributes ? attributes.OfType<ImagePreviewAttribute>().SingleOrDefault() : null;
                    var formAttr = hasAttributes ? attributes.OfType<FormDisplayAttribute>().SingleOrDefault() : null;

                    if (formAttr == null)
                    {
                        // check if it should be included for display
                        if (!ignoreUndecorated && !hasAttributes)
                        {
                            formAttr = defaultDisplay.CreateDefault();
                            attributes.Add(formAttr);
                        }
                    }

                    var hasInputFile = inputFileAttr != null || imageAttr != null;

                    if (imageAttr == null && (hasInputFile || true == formAttr?.UITypeHint.EqualsIgnoreCase("file")))
                    {
                        if (!pi.PropertyType.IsString() && !pi.PropertyType.IsNonStringEnumerableOf(typeof(string)))
                        {
                            throw new NotSupportedException(
                                $"Property '{pi.Name}' for file input must be of type string." +
                                $"The current type {pi.PropertyType} is unsupported");
                        }
                    }

                    if (formAttr != null)
                    {
                        if (formAttr.ColumnCssClass == null)
                            formAttr.ColumnCssClass = defaultDisplay.ColumnCssClass;

                        if (formAttr.CustomRenderMode == CustomRenderMode.Default)
                            formAttr.CustomRenderMode = defaultDisplay.CustomRenderMode;

                        if (formAttr.InputCssClass == null)
                        {
                            var css = defaultDisplay.InputCssClass ?? emptyDisplay.InputCssClass;
                            if (css == form_control)
                            {
                                // don't add by default 'form-control' class to the following input types
                                if (!__isRadioOrCheckbox()) formAttr.InputCssClass = css;
                            }
                            else formAttr.InputCssClass = css;
                        }
                        else if (formAttr.InputCssClass == form_control && __isRadioOrCheckbox())
                            formAttr.InputCssClass = null;

                        if (hasInputFile)
                        {
                            formAttr.UITypeHint = "file";

                            // give priority to ImagePreviewAttribute as it inherits InputFileAttribute
                            formAttr.SetFileAttribute(imageAttr ?? inputFileAttr);

                            if (imageAttr != null && imageAttr.TargetElementId == null)
                            {
                                imageAttr.TargetElementId = $"{pi.Name}{ImagePreviewAttribute.TargetElementIdSuffix}";
                            }
                        }

                        formAttr.SetDragDropAttribute(dragDropAttr);
                        formAttr.SetProperty(pi);

                        bool __isRadioOrCheckbox()
                            => formAttr.IsInputCheckboxOrRadio || pi.PropertyType.IsBoolean();
                    }

                    attributeBag.AddRange(pi.Name, attributes);
                }

                result = attributeBag.CreateGroupedDisplays(defaultDisplay);
                _sectionLayoutMetadataCache[fullpath] = attributeBag;
            }
            else
            {
                result = attributeBag.FormDisplayAttributeGroups;
            }

            return result != null;
        }

        /// <summary>
        /// Attempts to get a cached instance of the <see cref="AutoInputMetadata"/>
        /// class whose <see cref="AutoInputMetadata.PropertyInfo"/>.Name matches the
        /// specified <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="type">The type of the object to which <paramref name="propertyName"/> belongs.</param>
        /// <param name="propertyName">The name of the property for which to retrieve the metadata.</param>
        /// <param name="result">Returns the cached <see cref="AutoInputMetadata"/>, if any.</param>
        /// <returns></returns>
        public static bool TryGetMetadata(this Type type, string propertyName, out AutoInputMetadata result)
        {
            result = null;
            if (type is null) return false;

            if (_sectionLayoutMetadataCache.TryGetValue(type.GetNestedTypeHash(), out var bag))
            {
                foreach (var group in bag.FormDisplayAttributeGroups)
                    foreach (var metadata in group.Items)
                        if (metadata.PropertyInfo.Name == propertyName)
                        {
                            result = metadata;
                            return true;
                        }
            }

            return false;
        }

        /// <summary>
        /// Attempts to get the metadata value associated with the specified type.
        /// </summary>
        /// <param name="type">The type of the object from which to extract metadata.</param>
        /// <param name="result">Returns a grouped collection of <see cref="AutoInputMetadata"/> objects.</param>
        /// <returns></returns>
        public static bool TryGetMetadata(this Type type, out IReadOnlyCollection<FormDisplayGroupMetadata> result)
        {
            if (_sectionLayoutMetadataCache.TryGetValue(type.GetNestedTypeHash(), out var bag))
                result = bag.FormDisplayAttributeGroups;
            else
                result = null;
            return result != null;
        }

        /// <summary>
        /// Make sure that properties decorated with an ignorable 
        /// custom attribute are NOT included for serialization.
        /// </summary>
        /// <param name="obj">The object to scan.</param>
        /// <returns>
        /// An initialized instance of the <see cref="ObjectDictionary"/> class if 
        /// there are properties to be ignored; otherwise, <paramref name="obj"/>.
        /// </returns>
        public static object RemoveIgnoredPropertiesFromObjectToBeSerialized(this object obj)
        {
            if (obj != null && obj.GetType().TryGetMetadata(out var metadata))
            {
                var dic = new ObjectDictionary(obj);
                var different = false;
                foreach (var group in metadata)
                    foreach (var item in group.Items)
                    {
                        var attr = item.Attribute;
                        var name = attr.GetProperty().Name;
                        if (attr.IsIgnored() && dic.ContainsKey(name))
                        {
                            dic.Remove(name);
                            different = true;
                        }
                    }
                if (different) return dic;
            }
            return obj;
        }

        internal static bool TryGetAttribute<T>(this Type type, string propertyName, out T result) where T : FormAttributeBase
        {
            if (type != null && !string.IsNullOrWhiteSpace(propertyName) &&
                _sectionLayoutMetadataCache.TryGetValue(type.GetNestedTypeHash(), out var attributeBag))
                result = attributeBag.GetAttributeOfType<T>(propertyName);
            else
                result = null;

            return result != null;
        }

        /// <summary>
        /// Attempts to retrieve a cached collection of <see cref="FormAttributeBase"/>
        /// class instances for the specified type and property.
        /// </summary>
        /// <param name="type">The type to look up.</param>
        /// <param name="propertyName">The property name of the <paramref name="type"/> to look up.</param>
        /// <param name="filterByAttributeTypes">The types to filter by.</param>
        /// <returns></returns>
        public static IEnumerable<FormAttributeBase> GetAttributes(this Type type, string propertyName, params Type[] filterByAttributeTypes)
        {
            IEnumerable<FormAttributeBase> result = null;

            if (type != null &&
                !string.IsNullOrWhiteSpace(propertyName) &&
                _sectionLayoutMetadataCache.TryGetValue(type.GetNestedTypeHash(), out var attributeBag)
            )
            {
                result = attributeBag.GetAttributes(propertyName, filterByAttributeTypes);
            }

            return result ?? Enumerable.Empty<FormAttributeBase>();
        }
    }
}
