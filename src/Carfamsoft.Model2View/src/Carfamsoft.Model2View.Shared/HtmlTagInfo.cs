using Carfamsoft.Model2View.Shared.Extensions;
using System;

namespace Carfamsoft.Model2View.Shared
{
    /// <summary>
    /// Provides information about HTML tags.
    /// </summary>
    public static class HtmlTagInfo
    {

        private static readonly string[] SelfClosingTagNames = new string[]
        {
            "area",
            "base",
            "br",
            "col",
            "embed",
            "hr",
            "img",
            "input",
            "link",
            "meta",
            "param",
            "source",
            "track",
            "wbr",
        };

        private static readonly string[] TagsWithTypeAttribute = new string[]
        {
            "a",
            "button",
            "embed",
            "input",
            "link",
            "menu",
            "object",
            "script",
            "source",
            "style",
        };


        /// <summary>
        /// Determines whether the specified tag name is a self-closing HTML element.
        /// </summary>
        /// <param name="tagName">The name of the HTML tag.</param>
        /// <returns></returns>
        public static bool IsSelfClosing(string tagName) => SelfClosingTagNames.ContainsIgnoreCase(tagName);

        /// <summary>
        /// Determines whether the specified tag name supports the 'type' attribute.
        /// </summary>
        /// <param name="tagName">The name of the HTML tag.</param>
        /// <returns></returns>
        public static bool HasTypeAttribute(string tagName) => TagsWithTypeAttribute.ContainsIgnoreCase(tagName);


        /// <summary>
        /// Translates the system <paramref name="type"/> to an HTML control type.
        /// </summary>
        /// <param name="type">The system type to translate.</param>
        /// <returns></returns>
        public static string GetControlType(this Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "checkbox";

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return "number";

                case TypeCode.DateTime:
                    return "date";

                case TypeCode.Char:
                case TypeCode.String:
                    return "text";

                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return null;

                case TypeCode.Object:
                default:
                    return "text";
            }
        }
    }
}
