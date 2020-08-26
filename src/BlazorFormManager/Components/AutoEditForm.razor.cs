using BlazorFormManager.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;

namespace BlazorFormManager.Components
{
    /// <summary>
    /// Represents a form that automatically generates inputs based on declarative layout specifications.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    public abstract class AutoEditFormBase<TModel> : FormManagerBase<TModel>
    {
        /// <summary>
        /// Gets the collection of <see cref="FormDisplayGroupMetadata"/> retrieved from
        /// layout attributes of the associated model.
        /// </summary>
        protected IReadOnlyCollection<FormDisplayGroupMetadata> DisplayGroups { get; private set; }

        /// <summary>
        /// Gets or sets the form header render fragment.
        /// </summary>
        [Parameter] public RenderFragment FormHeader { get; set; }

        /// <summary>
        /// Gets or sets the form footer render fragment.
        /// </summary>
        [Parameter] public RenderFragment FormFooter { get; set; }

        /// <summary>
        /// Gets or sets the content rendered before the <see cref="FormDisplayGroup"/>s.
        /// </summary>
        [Parameter] public RenderFragment BeforeDisplayGroups { get; set; }

        /// <summary>
        /// Gets or sets the content rendered after the <see cref="FormDisplayGroup"/>s.
        /// </summary>
        [Parameter] public RenderFragment AfterDisplayGroups { get; set; }

        /// <inheritdoc/>
        protected override void NotifyModelChanged()
        {
            if (!Equals(default, Model) && Model.ExtractMetadata(out var groups))
            {
                DisplayGroups = groups;
                StateHasChanged();
            }
            base.NotifyModelChanged();
        }

        /// <summary>
        /// This method does nothing on purpose to avoid attaching field 
        /// change notifications to the underlying <see cref="EditContext"/>.
        /// <see cref="EditContext.OnFieldChanged"/> currently doesn't provide the 
        /// changed value. <see cref="AutoInputBase"/> makes it possible to detect both 
        /// field changes and the corresponding value that was changed.
        /// </summary>
        protected override void AttachFieldChangedListener()
        {
            // Don't attach to the underlying EditContext!
        }

        /// <summary>
        /// This method does nothing on purpose to avoid detaching from something that 
        /// was never attached to.
        /// </summary>
        protected override void DetachFieldChangedListener()
        {
            // Do nothing!
        }
    }
}
