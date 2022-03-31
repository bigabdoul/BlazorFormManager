using System;
using System.Collections.Generic;
using System.Reflection;

namespace Carfamsoft.Model2View.Shared
{
    /// <summary>
    /// A function callback used to retrieve a collection <see cref="SelectOption"/> elements.
    /// </summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    public delegate IEnumerable<SelectOption> OptionsGetterDelegate(PropertyInfo propertyInfo);

    /// <summary>
    /// Represents an object that controls a part of an HTML generation process.
    /// </summary>
    public class ControlRenderOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControlRenderOptions"/> class.
        /// </summary>
        public ControlRenderOptions()
        {
        }

        /// <summary>
        /// Returns a new instance if the <see cref="ControlRenderOptions"/> class using default values.
        /// </summary>
        /// <param name="generateName">Indicates whether the 'name' attribute for a control should be added.</param>
        /// <returns></returns>
        public static ControlRenderOptions CreateDefault(bool generateName = false) => new ControlRenderOptions
        {
            RowCssClass = "row",
            DefaultCssClass = "form-control",
            GenerateIdAttribute = true,
            CamelCaseId = true,
            //PredictableId = true,
            EnableActivatorCreateInstance = true,
            GenerateNameAttribute = generateName,
        };

        /// <summary>
        /// Gets the default control render options.
        /// </summary>
        public static readonly ControlRenderOptions Default = CreateDefault();

        /// <summary>
        /// Gets or sets the CSS class name for a grouped row.
        /// </summary>
        public string RowCssClass { get; set; }

        /// <summary>
        /// Gets or sets the CSS class name for a column in a grouped row.
        /// </summary>
        public string ColCssClass { get; set; }

        /// <summary>
        /// Gets or sets the HTML tag name for the element that wraps around a group of properties.
        /// </summary>
        public string GroupWrapperTagName { get; set; }

        /// <summary>
        /// Gets or sets the HTML tag name for the element that acts a the title for the group.
        /// </summary>
        public string GroupHeaderTagName { get; set; }

        /// <summary>
        /// Gets or sets the CSS class name for a group.
        /// </summary>
        public string GroupNameCssClass { get; set; }

        /// <summary>
        /// Gets or sets the CSS class name for a property.
        /// </summary>
        public string PropertyNameCssClass { get; set; }

        /// <summary>
        /// Indicates whether the 'id' attribute for a control should be computed.
        /// </summary>
        public bool GenerateIdAttribute { get; set; }

        /// <summary>
        /// Indicates whether the 'name' attribute for a control should be added.
        /// </summary>
        public bool GenerateNameAttribute { get; set; }

        /// <summary>
        /// Indicates whether <see cref="Activator.CreateInstance(Type)"/> should
        /// be invoked for nested reference-typed (class) properties (except 
        /// <see cref="string"/>) that have not been initialized within a model. 
        /// </summary>
        public bool EnableActivatorCreateInstance { get; set; }

        /// <summary>
        /// Gets or sets the default CSS class for a rendered control.
        /// </summary>
        public string DefaultCssClass { get; set; }

        /// <summary>
        /// Gets or sets a function callback used to retrieve a collection <see cref="SelectOption"/>
        /// elements to use for a 'select' element or an input or type 'radio'.
        /// </summary>
        public OptionsGetterDelegate OptionsGetter { get; set; }

        /// <summary>
        /// Gets or sets a function callback used to dynamically retrieve the disabled state of an input.
        /// </summary>
        public Func<PropertyInfo, bool?> DisabledGetter { get; set; }
        
        /// <summary>
        /// Gets or sets a function callback used to retrieve additional attributes.
        /// </summary>
        public Func<HtmlRenderInfo, IReadOnlyDictionary<string, object>> AdditionalAttributesGetter { get; set; }

        /// <summary>
        /// Gets or sets the unique control identifier.
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// Determines whether to use camel-casing when generating the 'id' attribute.
        /// </summary>
        public bool CamelCaseId { get; set; }

        /*
        /// <summary>
        /// Indicates that control identifiers should be generated in a predictable manner.
        /// </summary>
        public bool PredictableId { get; set; }
        */
    }
}
