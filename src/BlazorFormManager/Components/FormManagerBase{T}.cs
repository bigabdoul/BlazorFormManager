using Microsoft.AspNetCore.Components;

namespace BlazorFormManager.Components
{
    /// <summary>
    /// Implements a strongly-typed <see cref="FormManagerBase"/> class.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class FormManagerBase<TModel> : FormManagerBase
    {
        /// <summary>
        /// Gets or sets the form model to upload.
        /// </summary>
        [Parameter] public TModel Model { get; set; }

        /// <summary>
        /// Returns the <see cref="Model"/> property value.
        /// </summary>
        /// <returns></returns>
        public override object GetModel() => Model;
    }
}
