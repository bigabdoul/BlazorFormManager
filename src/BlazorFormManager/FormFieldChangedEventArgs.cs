using Microsoft.AspNetCore.Components.Forms;
using System;

namespace BlazorFormManager
{
    /// <summary>
    /// Encapsulates data related to an event raised when a form field changes.
    /// </summary>
    public class FormFieldChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FormFieldChangedEventArgs"/>
        /// </summary>
        /// <param name="isValid">true if the form is currently valid; otherwise, false.</param>
        /// <param name="fieldIdentifier">The field whose value has changed.</param>
        public FormFieldChangedEventArgs(bool isValid, FieldIdentifier fieldIdentifier)
        {
            IsValid = isValid;
            Field = fieldIdentifier;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormFieldChangedEventArgs"/>
        /// class using the specified parameters.
        /// </summary>
        /// <param name="value">The value of the field that changed.</param>
        /// <param name="fieldIdentifier">The field whose value has changed.</param>
        /// <param name="isFile">Indicates whether the field that triggered the change is an input file.</param>
        public FormFieldChangedEventArgs(object value, FieldIdentifier fieldIdentifier, bool isFile = false)
        {
            Value = value;
            Field = fieldIdentifier;
            IsFile = isFile;
        }

        /// <summary>
        /// Indicates whether the current underlying <see cref="EditContext"/> is valid.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Identifies the field whose value has changed.
        /// </summary>
        public FieldIdentifier Field { get; }

        /// <summary>
        /// Gets the value of the field that changed.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Indicates whether the field that triggered the change is an input file.
        /// </summary>
        public bool IsFile { get; }

        internal void SetValue(object value)
        {
            Value = value;
        }
    }
}
