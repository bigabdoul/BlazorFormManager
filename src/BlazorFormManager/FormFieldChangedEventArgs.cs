using BlazorFormManager.ComponentModel.ViewAnnotations;
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
        /// <param name="elementId">The identifier of the element that triggered the change.</param>
        /// <param name="fileAttribute">A copy of the custom attribute attached to the current field, if any.</param>
        public FormFieldChangedEventArgs(object value, FieldIdentifier fieldIdentifier, bool isFile = false, string elementId = null, InputFileAttribute fileAttribute = null)
        {
            Value = value;
            Field = fieldIdentifier;
            IsFile = isFile;
            FieldId = elementId;
            FileAttribute = fileAttribute;
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
        /// Gets the identifier of the field that triggered the change.
        /// </summary>
        public string FieldId { get; }

        /// <summary>
        /// Gets the value of the field that changed.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Gets a copy of the custom attribute attached to the current field, if any.
        /// </summary>
        public InputFileAttribute FileAttribute { get; }

        /// <summary>
        /// Indicates whether the field that triggered the change is an input file.
        /// </summary>
        public bool IsFile { get; }

        /// <summary>
        /// Indicates whether this instance has a valid file.
        /// </summary>
        public bool HasFile => FileAttribute != null && FileAttribute.Method != FileReaderMethod.None && !string.IsNullOrWhiteSpace(Value?.ToString());

        /// <summary>
        /// Checks if the current instance refers to a non-empty file matching the specified field name.
        /// </summary>
        /// <param name="fieldName">The name of the field to compare against.</param>
        /// <returns></returns>
        public bool IsEmptyFile(string fieldName)
        {
            return IsFile && Field.FieldName == fieldName && string.IsNullOrWhiteSpace(Value?.ToString());
        }
    }
}
