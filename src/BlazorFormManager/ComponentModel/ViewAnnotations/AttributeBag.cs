using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BlazorFormManager.ComponentModel.ViewAnnotations
{
    internal sealed class AttributeBag
    {
        private readonly ConcurrentDictionary<string, FormAttributeBase[]> _dictionary
            = new ConcurrentDictionary<string, FormAttributeBase[]>();

        private readonly List<string> _propertyNames = new List<string>();

        internal IReadOnlyCollection<FormDisplayGroupMetadata> FormDisplayAttributeGroups { get; private set; }

        internal bool AddRange(string propertyName, IEnumerable<FormAttributeBase> collection)
        {
            if (_dictionary.TryAdd(propertyName, collection.ToArray()))
            {
                // preserve the order in which properties are declared in the object instance
                _propertyNames.Add(propertyName);
            }
            return false;
        }

        internal IReadOnlyCollection<FormDisplayGroupMetadata> CreateGroupedDisplays(FormDisplayDefaultAttribute defaultDisplay, object instance)
        {
            if (FormDisplayAttributeGroups == null)
            {
                // collect all custom attributes of type FormDisplayAttribute;
                // only these attributes are used to display HTML elements
                var formDisplayAttributes = new List<FormDisplayAttribute>();

                foreach (var propertyName in _propertyNames)
                {
                    foreach (var attr in _dictionary[propertyName])
                        if (attr is FormDisplayAttribute displayAttribute)
                            formDisplayAttributes.Add(displayAttribute);
                }

                var attrGroups = formDisplayAttributes
                    .OrderBy(a => a.Order)
                    .GroupBy(attr => attr.GroupName);
                
                var groups = new List<FormDisplayGroupMetadata>();

                foreach (var attributes in attrGroups)
                {
                    var groupData = new FormDisplayGroupMetadata
                    {
                        Name = attributes.Key,
                        CssClass = defaultDisplay.GroupCssClass,
                        ShowName = !string.IsNullOrWhiteSpace(attributes.Key) && true == defaultDisplay?.ShowGroupName,
                    };

                    groups.Add(groupData);

                    var items = groupData.Items;

                    foreach (var attr in attributes)
                    {
                        items.Add(new AutoInputMetadata(instance, attr));
                    }
                }

                FormDisplayAttributeGroups = groups.AsReadOnly();
                formDisplayAttributes.Clear();
                _propertyNames.Clear();
            }

            return FormDisplayAttributeGroups;
        }

        internal T GetAttributeOfType<T>(string propertyName) where T : FormAttributeBase
            => _dictionary.TryGetValue(propertyName, out var formAttributes) 
                ? formAttributes.OfType<T>().FirstOrDefault() : null;

        internal IEnumerable<FormAttributeBase> GetAttributes(string propertyName, params Type[] attributeTypes)
        {
            if (_dictionary.TryGetValue(propertyName, out var attributes))
            {
                var hasFilter = attributeTypes.Length > 0;

                foreach (var attr in attributes)
                {
                    if (!hasFilter) 
                        yield return attr;
                    else
                    {
                        foreach (var type in attributeTypes)
                        {
                            if (attr.GetType() == type) yield return attr;
                        }
                    }
                }
            }
        }
    }
}
