using System.Reflection;

namespace Carfamsoft.Model2View.Shared
{
    /// <summary>
    /// Represents an object that encapsulates information related to an HTML attribute.
    /// This class is commonly used in conjunction with a function that supplies additional
    /// attributes when an element is being rendered. This class cannot be inherited.
    /// </summary>
    public sealed class HtmlRenderInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlRenderInfo"/> class.
        /// </summary>
        public HtmlRenderInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlRenderInfo"/> 
        /// class using the specified parameters.
        /// </summary>
        /// <param name="property">Reflected information about the property being rendered.</param>
        /// <param name="propertyName">The fully-qualified property name of a model being rendered.</param>
        /// <param name="elementType">The HTML tag or element name (e.g. 'input', 'textarea', etc.).</param>
        /// <param name="tagName">The HTML tag or element name (e.g. 'input', 'textarea', etc.).</param>
        public HtmlRenderInfo(PropertyInfo property, string propertyName, string elementType, string tagName)
        {
            Property = property;
            PropertyName = propertyName;
            ElementType = elementType;
            TagName = tagName;
        }

        /// <summary>
        /// Gets the property being rendered.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets the property name of a model being rendered.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the HTML element type (e.g. 'checkbox', 'file', etc.).
        /// </summary>
        public string ElementType { get; }

        /// <summary>
        /// Gets the HTML tag or element name (e.g. 'input', 'textarea', etc.).
        /// </summary>
        public string TagName { get; }
    }
}
