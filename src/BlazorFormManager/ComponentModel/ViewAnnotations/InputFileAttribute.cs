using System;

namespace BlazorFormManager.ComponentModel.ViewAnnotations
{
    /// <summary>
    /// Represents a custom attribute that is used to generate inputs of type file and
    /// provides settings for file reading using JavaScript's FileReader API.
    /// </summary>
    public class InputFileAttribute : Attribute, ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputFileAttribute"/> class.
        /// </summary>
        public InputFileAttribute()
        {
        }

        /// <summary>
        /// Gets or sets a comma-separated list of file name extensions that limits the 
        /// types of files a user can pick. If the value is null or empty (or only 
        /// whitespace) then any file can be picked.
        /// </summary>
        public string Accept { get; set; }

        /// <summary>
        /// Gets or sets the type of file that can be picked up.
        /// </summary>
        public string AcceptType { get; set; }

        /// <summary>
        /// Indicates whether multiple file selection is allowed.
        /// </summary>
        public bool Multiple { get; set; }

        /// <summary>
        /// Gets or sets the method of the FileReader API (in JavaScript) to use.
        /// </summary>
        public FileReaderMethod Method { get; set; }

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
