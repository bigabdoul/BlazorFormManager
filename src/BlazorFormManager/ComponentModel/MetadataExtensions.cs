using BlazorFormManager.ComponentModel.ViewAnnotations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BlazorFormManager.ComponentModel
{
    /// <summary>
    /// Provides extension methods for extracting and manipulating metadata, such as custom attributes.
    /// </summary>
    public static class MetadataExtensions
    {
        private static readonly ConcurrentDictionary<Type, IReadOnlyCollection<FormDisplayGroupMetadata>> _sectionLayoutMetadataCache
            = new ConcurrentDictionary<Type, IReadOnlyCollection<FormDisplayGroupMetadata>>();

        private const string form_control = "form-control";

        /// <summary>
        /// Generates a grouped collection of <see cref="AutoInputMetadata"/> instances
        /// extracted from custom attributes of types <see cref="FormDisplayAttribute"/>
        /// and <see cref="FormDisplayDefaultAttribute"/> that decorate the specified 
        /// <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">The type of the object from which to extract metadata.</typeparam>
        /// <param name="instance">An instance of the specified <typeparamref name="T"/> type.</param>
        /// <param name="result">Returns a grouped collection of <see cref="AutoInputMetadata"/> objects.</param>
        /// <returns></returns>
        public static bool ExtractMetadata<T>(this T instance, out IReadOnlyCollection<FormDisplayGroupMetadata> result)
            => typeof(T).ExtractMetadata(instance, out result);

        /// <summary>
        /// Generates a grouped collection of <see cref="AutoInputMetadata"/> instances
        /// extracted from custom attributes of types <see cref="FormDisplayAttribute"/>
        /// and <see cref="FormDisplayDefaultAttribute"/> that decorate the specified 
        /// <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of the object from which to extract metadata.</param>
        /// <param name="instance">An instance of the specified <paramref name="type"/>.</param>
        /// <param name="result">Returns a grouped collection of <see cref="AutoInputMetadata"/> objects.</param>
        /// <returns></returns>
        public static bool ExtractMetadata(this Type type, object instance, out IReadOnlyCollection<FormDisplayGroupMetadata> result)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            if (!_sectionLayoutMetadataCache.TryGetValue(type, out result))
            {
                var layouts = new List<FormDisplayAttribute>();
                var emptyDisplay = FormDisplayAttribute.Empty;
                var properties = type.GetProperties();
                var ignoreUndecorated = type.GetCustomAttribute<DisplayIgnoreAttribute>() != null;
                var layoutDefault = type.GetCustomAttributes<FormDisplayDefaultAttribute>().SingleOrDefault() ?? FormDisplayDefaultAttribute.Empty;
                
                foreach (var pi in properties)
                {
                    // properties must be read-write
                    if (!(pi.CanRead && pi.CanWrite)) continue;
                    if (pi.GetCustomAttribute<DisplayIgnoreAttribute>(true) != null) continue;

                    var attr = pi.GetCustomAttribute<FormDisplayAttribute>(true);
                    var imageAttr = pi.GetCustomAttribute<ImagePreviewAttribute>(true);
                    var inputFile = pi.GetCustomAttribute<InputFileAttribute>(true);
                    var hasInputFile = inputFile != null || imageAttr != null;

                    if ((hasInputFile || attr?.UITypeHint == "file") && !pi.PropertyType.IsString())
                        throw new NotSupportedException(
                            $"Property '{pi.Name}' for file input must be of type string." +
                            $"The current type {pi.PropertyType} is unsupported");

                    if (attr == null)
                    {
                        if (!hasInputFile && ignoreUndecorated) continue;
                        attr = layoutDefault.CreateDefault();
                    }    
                    else if (attr.ColumnCssClass == null) 
                        attr.ColumnCssClass = layoutDefault.ColumnCssClass;

                    if (attr.CustomRenderMode == CustomRenderMode.Default)
                        attr.CustomRenderMode = layoutDefault.CustomRenderMode;

                    if (attr.InputCssClass == null)
                    {
                        var css = layoutDefault.InputCssClass ?? emptyDisplay.InputCssClass;
                        if (css == form_control)
                        {
                            // don't add by default 'form-control' class to the following input types
                            if (!__isRadioOrCheckbox()) attr.InputCssClass = css;
                        }
                        else attr.InputCssClass = css;
                    }
                    else if (attr.InputCssClass == form_control && __isRadioOrCheckbox())
                        attr.InputCssClass = null;

                    if (hasInputFile)
                    {
                        attr.UITypeHint = "file";

                        // give priority to ImagePreviewAttribute
                        attr.FileAttribute = imageAttr ?? inputFile;

                        if (imageAttr != null && imageAttr.TargetElementId == null)
                        {
                            imageAttr.TargetElementId = $"{pi.Name}{ImagePreviewAttribute.TargetElementIdSuffix}";
                        }
                    }

                    attr.SetProperty(pi);
                    layouts.Add(attr);

                    bool __isRadioOrCheckbox()
                        => attr.IsInputCheckboxOrRadio || pi.PropertyType.IsBoolean();
                }

                if (layouts.Count > 0)
                {
                    var attrGroups = layouts.OrderBy(a => a.Order).GroupBy(attr => attr.GroupName);
                    var groups = new List<FormDisplayGroupMetadata>();

                    foreach (var attributes in attrGroups)
                    {
                        var groupData = new FormDisplayGroupMetadata
                        {
                            Name = attributes.Key,
                            CssClass = layoutDefault.GroupCssClass,
                            ShowName = !string.IsNullOrWhiteSpace(attributes.Key) && true == layoutDefault?.ShowGroupName,
                        };

                        groups.Add(groupData);

                        var items = groupData.Items;

                        foreach (var attr in attributes)
                        {
                            items.Add(new AutoInputMetadata(instance, attr));
                        }
                    }

                    result = groups.AsReadOnly();
                    _sectionLayoutMetadataCache[type] = result;
                }
            }
            else
            {
                // refresh the model in all instances of AutoInputMetadata
                foreach (var group in result)
                    foreach (var metadata in group.Items)
                        metadata.Refresh(instance);
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

            if (_sectionLayoutMetadataCache.TryGetValue(type, out var meta))
            {
                foreach (var group in meta)
                    foreach (var metadata in group.Items)
                        if (metadata.PropertyInfo.Name == propertyName)
                        {
                            result = metadata;
                            return true;
                        }
            }

            return false;
        }
    }
}
