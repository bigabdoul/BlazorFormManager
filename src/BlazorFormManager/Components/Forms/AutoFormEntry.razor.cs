using Carfamsoft.Model2View.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorFormManager.Components.Forms
{
    /// <summary>
    /// Base class for form entry fields.
    /// </summary>
    public abstract class AutoFormEntryBase : ComponentBase
    {
        private FieldIdentifier _fieldIdentifier;
        
        /// <summary>
        /// Gets or sets the element identifier.
        /// </summary>
        [Parameter] public string? Id { get; set; }
        
        /// <summary>
        /// Gets or sets the name attribute value.
        /// </summary>
        [Parameter] public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        [Parameter] public object? Model { get; set; }

        /// <summary>
        /// Gets or sets the metadata for the input.
        /// </summary>
        [Parameter] public AutoInputMetadata? Metadata { get; set; }

        /// <summary>
        /// Gets or sets the navigation path to the property 
        /// that renders the current <see cref="AutoFormEntry"/>.
        /// </summary>
        [Parameter] public string PropertyNavigationPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets the associated <see cref="FormManagerBase"/>.
        /// </summary>
        [CascadingParameter] protected FormManagerBase? Form { get; set; }

        /// <summary>
        /// Gets the input's identifier.
        /// </summary>
        protected string InputId { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the input's name.
        /// </summary>
        protected string InputName { get; private set;} = string.Empty;

        /// <inheritdoc/>
        protected override void OnParametersSet()
        {
            if (Metadata == null)
                throw new System.ArgumentNullException(nameof(Metadata));

            (InputId, InputName) = Metadata.GenerateIdAndName(Form?.RenderOptions, PropertyNavigationPath, Id);

            if (Model != null)
            {
                _fieldIdentifier = new FieldIdentifier(Model, Metadata.PropertyInfo.Name);
            }
        }

        /// <summary>
        /// Gets or sets the model's value.
        /// </summary>
        protected virtual object? Value
        {
            get => Metadata?.GetValue(Model);
            set
            {
                if (HasValueChanged(value, out var changedEventArgs))
                {
                    Metadata!.SetValue(Model, value);
                    Form?.NotifyFieldChanged(changedEventArgs!);
                }
            }
        }

        /// <summary>
        /// Attempts to update a matching property in the <see cref="Model"/>.
        /// </summary>
        /// <param name="value">The new value to set.</param>
        /// <param name="result">Returns the form field changed event arguments if the property is updated.</param>
        /// <returns></returns>
        protected bool HasValueChanged(object? value, out FormFieldChangedEventArgs? result)
        {
            return AutoInputBase.GetNotifyFieldChangedArgs(out result, Value, value, Metadata, InputId, _fieldIdentifier);
        }
    }
}
