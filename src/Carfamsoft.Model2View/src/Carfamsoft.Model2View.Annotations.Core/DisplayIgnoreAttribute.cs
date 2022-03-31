using System;

namespace Carfamsoft.Model2View.Annotations
{
    /// <summary>
    /// Prevents a property from being included into the input element generation process.
    /// If this attribute is applied to a class, all public read/write properties that
    /// aren't decorated with <see cref="FormDisplayAttribute"/> will be ignored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DisplayIgnoreAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayIgnoreAttribute"/> class.
        /// </summary>
        public DisplayIgnoreAttribute()
        {
        }
    }
}
