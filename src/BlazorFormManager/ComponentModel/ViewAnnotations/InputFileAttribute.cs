using System;

namespace BlazorFormManager.ComponentModel.ViewAnnotations
{
    /// <summary>
    /// Represents a custom attribute that is used to generate inputs of type file and
    /// provides settings for file reading using JavaScript's FileReader API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class InputFileAttribute : FileCapableAttributeBase, ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputFileAttribute"/> class.
        /// </summary>
        public InputFileAttribute()
        {
        }

        /// <summary>
        /// Creates and returns a shallow copy of the current 
        /// <see cref="InputFileAttribute"/> instance.
        /// </summary>
        /// <returns></returns>
        public virtual InputFileAttribute Clone()
        {
            return (InputFileAttribute)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
