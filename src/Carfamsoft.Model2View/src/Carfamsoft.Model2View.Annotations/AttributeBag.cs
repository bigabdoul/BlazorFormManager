using Carfamsoft.Model2View.Shared.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Carfamsoft.Model2View.Annotations
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

        internal IReadOnlyCollection<FormDisplayGroupMetadata> CreateGroupedDisplays(FormDisplayDefaultAttribute defaultDisplay)
        {
            if (FormDisplayAttributeGroups == null)
            {
                // collect all custom attributes of type FormDisplayAttribute;
                // only these attributes are used to display HTML elements
                var formDisplayAttributes = new List<FormDisplayAttribute>();

                var resourceManager = defaultDisplay?.GetResourceManager();

                foreach (var propertyName in _propertyNames)
                {
                    foreach (var attr in _dictionary[propertyName])
                        if (attr is FormDisplayAttribute displayAttribute)
                            formDisplayAttributes.Add(displayAttribute);
                }

                // order priority goes by the lowest numbers
                var groups1 = formDisplayAttributes.Where(a => a.Order < 1).OrderBy(a => a.Order).GroupBy(a => a.GroupName);
                
                // order the rest...
                var groups2 = formDisplayAttributes.Where(a => a.Order > 0).OrderBy(a => a.Order).GroupBy(a => a.GroupName);

                // add them to a grouped list
                var attrList = new List<IGrouping<string, FormDisplayAttribute>>();

                attrList.AddRange(groups1);
                attrList.AddRange(groups2);

                var groups = new List<FormDisplayGroupMetadata>();

                foreach (var attributes in attrList)
                {
                    var groupDescription = attributes.FirstOrDefault(a => a.GroupDescription.IsNotBlank())?.GroupDescription;
                    groupDescription = resourceManager?.GetDisplayString(groupDescription) ?? groupDescription;
                    var showDescr = defaultDisplay == null || defaultDisplay.ShowGroupDescription;
                    var groupData = new FormDisplayGroupMetadata
                    {
                        Name = resourceManager?.GetDisplayString(attributes.Key) ?? attributes.Key,
                        CssClass = defaultDisplay?.GroupCssClass,
                        ShowName = !string.IsNullOrWhiteSpace(attributes.Key) && true == defaultDisplay?.ShowGroupName,
                        Icon = attributes.FirstOrDefault(a => a.GroupIcon.IsNotBlank())?.GroupIcon ?? defaultDisplay?.GroupIcon,
                        Description = groupDescription,
                        ShowDescription = showDescr && groupDescription.IsNotBlank(),
                    };

                    groups.Add(groupData);

                    var items = groupData.Items;

                    foreach (var attr in attributes)
                    {
                        items.Add(new AutoInputMetadata(attr, attr.GetResourceManager() ?? resourceManager));
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

        internal IEnumerable<FormAttributeBase> GetAttributes(string propertyName, params Type[] filterByAttributeTypes)
        {
            if (_dictionary.TryGetValue(propertyName, out var attributes))
            {
                var hasFilter = filterByAttributeTypes?.Length > 0;

                foreach (var attr in attributes)
                    if (!hasFilter)
                        yield return attr;
                    else
                        foreach (Type type in filterByAttributeTypes)
                            if (attr.GetType() == type)
                                yield return attr;
            }
        }
    }
}
