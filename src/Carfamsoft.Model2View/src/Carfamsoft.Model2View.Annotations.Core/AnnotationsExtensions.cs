using System.ComponentModel.DataAnnotations;

namespace Carfamsoft.Model2View.Annotations
{
    /// <summary>
    /// Provides extension methods data annotation objects.
    /// </summary>
    public static class AnnotationsExtensions
    {
        /// <summary>
        /// Returns a string that represents an HTML element type based on the specified <paramref name="dataType"/>.
        /// </summary>
        /// <param name="dataType">A <see cref="DataTypeAttribute"/> custom attribute to interpret.</param>
        /// <returns></returns>
        public static string GetControlType(this DataTypeAttribute dataType)
        {
            switch (dataType.DataType)
            {
                case DataType.DateTime:
                    return "datetime-local";
                case DataType.Date:
                    return "date";
                case DataType.Time:
                    return "time";
                case DataType.PhoneNumber:
                    return "tel";
                case DataType.EmailAddress:
                    return "email";
                case DataType.Password:
                    return "password";
                case DataType.Url:
                case DataType.ImageUrl:
                    return "url";
                case DataType.Upload:
                    return "file";
                case DataType.Custom:
                case DataType.Currency:
                case DataType.Duration:
                case DataType.Text:
                case DataType.Html:
                case DataType.MultilineText:
                case DataType.CreditCard:
                case DataType.PostalCode:
                default:
                    return "text";
            }
        }
    }
}
