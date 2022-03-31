using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;

namespace BlazorFormManager.Components.Forms
{
    /// <summary>
    /// <inheritdoc/>
    /// This class provides a way to initialize differently the underlying <see cref="FieldIdentifier"/> class.
    /// </summary>
    public class AutoValidationMessage : ValidationMessage<object>
    {
        /// <summary>
        /// Specifies the model for which validation messages should be displayed.
        /// </summary>
        [Parameter] public object? Model { get; set; }

        /// <summary>
        /// Specifies the field for which validation messages should be displayed.
        /// </summary>
        [Parameter] public string? Property { get; set; }

        /// <inheritdoc />
        protected override void OnParametersSet()
        {
            if (Model == null)
            {
                throw new InvalidOperationException(
                $"{GetType()} requires a value for the {nameof(Model)} parameter.");
            }
            if (string.IsNullOrWhiteSpace(Property))
            {
                throw new InvalidOperationException(
                $"{GetType()} requires a value for the {nameof(Property)} parameter.");
            }

            // This is required for the FieldIdentifier class initialization in order
            // to avoid throwing an exception when calling base.OnParametersSet()
            var prop = new { Property = $"{Model.GetType()}.{Property}" };
            For = () => prop.Property;
            base.OnParametersSet();

            InitFieldIdentifier(Model, Property!);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FieldIdentifier"/> class using the specified parameters.
        /// </summary>
        /// <param name="model">The object that owns the field.</param>
        /// <param name="fieldName">The name of the editable field.</param>
        protected virtual void InitFieldIdentifier(object model, string fieldName)
        {
            // The base class should allow overriding the way the field identifier is initialized.
            // Before we get there this is the only way to initialize the underlying FieldIdentifier.
            var field = typeof(ValidationMessage<object>).GetField("_fieldIdentifier", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            field?.SetValue(this, new FieldIdentifier(model, fieldName));
        }
    }
}
