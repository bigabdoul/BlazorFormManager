using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Threading.Tasks;

namespace BlazorFormManager.Components
{
    /// <summary>
    /// Implements a strongly-typed <see cref="FormManagerBase"/> class.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class FormManagerBase<TModel> : FormManagerBase
    {
        private TModel _model;

        /// <summary>
        /// Gets or sets the form model to upload.
        /// </summary>
        [Parameter] public TModel Model
        {
            get => _model;
            set
            {
                if (!Equals(value, _model))
                {
                    _model = value;
                    SetEditContext(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets a callback delegate that is invoked when a field in the form changes.
        /// </summary>
        [Parameter] public EventCallback<FieldChangedEventArgs> OnFieldChanged { get; set; }

        /// <summary>
        /// Returns the <see cref="Model"/> property value.
        /// </summary>
        /// <returns></returns>
        public override object GetModel() => Model;

        /// <summary>
        /// Gets the form's current edit context.
        /// </summary>
        protected EditContext EditContext { get; private set; }

        /// <summary>
        /// Indicates whether the form has unsaved changes.
        /// </summary>
        public bool HasChanges { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="result"><inheritdoc/></param>
        /// <returns></returns>
        protected override Task HandleSubmitDoneAsync(FormManagerSubmitResult result)
        {
            HasChanges = !result.Succeeded;
            return base.HandleSubmitDoneAsync(result);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"><inheritdoc/></param>
        protected override void Dispose(bool disposing)
        {
            RemoveEditContextHandler();
            base.Dispose(disposing);
        }

        #region helpers

        private void SetEditContext(TModel model)
        {
            if (!Equals(model, default))
            {
                RemoveEditContextHandler();
                EditContext = new EditContext(model);
                EditContext.OnFieldChanged += HandleFieldChanged;
                StateHasChanged();
            }
        }

        private void HandleFieldChanged(object sender, FieldChangedEventArgs e)
        {
            HasChanges = true;
            HasValidationErrors = !EditContext.Validate();
            StateHasChanged();

            if (OnFieldChanged.HasDelegate) OnFieldChanged.InvokeAsync(e);
        }

        private void RemoveEditContextHandler()
        {
            if (EditContext != null)
                EditContext.OnFieldChanged -= HandleFieldChanged;
        } 

        #endregion
    }
}
