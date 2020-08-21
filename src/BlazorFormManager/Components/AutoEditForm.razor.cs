using BlazorFormManager.ComponentModel;
using Microsoft.AspNetCore.Components;
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
        /// Gets the collection of <see cref="FormDisplayGroupMetadata"/> retrieved from layout attributes of the associated model.
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
    }
}
