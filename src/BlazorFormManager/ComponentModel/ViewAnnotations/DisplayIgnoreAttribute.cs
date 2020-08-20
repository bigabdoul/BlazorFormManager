using System;

namespace BlazorFormManager.ComponentModel.ViewAnnotations
{
    /// <summary>
    /// Prevents a property from being included into the input element generation process.
    /// </summary>
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
