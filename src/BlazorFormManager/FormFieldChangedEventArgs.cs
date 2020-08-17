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
        /// Indicates whether the current underlying <see cref="EditContext"/> is valid.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Identifies the field whose value has changed.
        /// </summary>
        public FieldIdentifier Field { get; set; }
    }
}
