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
        #region fields
        
        private TModel _model;
        private bool _hasChangeTracker;
        private bool _parametersSet;

        #endregion

        #region properties

        #region protected

        /// <summary>
        /// Gets the form's current edit context.
        /// </summary>
        protected EditContext EditContext { get; private set; }

        /// <summary>
        /// Indicates whether the form has unsaved changes.
        /// </summary>
        public bool HasChanges { get; private set; }

        #endregion

        #region parameters

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
                    SetEditContext();
                }
            }
        }

        /// <summary>
        /// Gets or sets a callback delegate that is invoked when a field in the form changes.
        /// </summary>
        [Parameter] public EventCallback<FormFieldChangedEventArgs> OnFieldChanged { get; set; }

        /// <summary>
        /// Indicates whether field changes should be tracked or not.
        /// </summary>
        [Parameter] public bool EnableChangeTracking { get; set; }

        #endregion

        #endregion

        #region methods

        /// <summary>
        /// Returns the <see cref="Model"/> property value.
        /// </summary>
        /// <returns></returns>
        public override object GetModel() => Model;

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
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            _parametersSet = true;
            if (EditContext == null) SetEditContext();
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

        #endregion

        #region helpers

        private void SetEditContext()
        {
            if (!_parametersSet) return;

            RemoveEditContextHandler();

            if (Equals(_model, default)) return;

            EditContext = new EditContext(_model);

            if (EnableChangeTracking)
            {
                EditContext.OnFieldChanged += HandleFieldChanged;
                _hasChangeTracker = true;
            }
                
            StateHasChanged();
        }

        private void HandleFieldChanged(object sender, FieldChangedEventArgs e)
        {
            HasChanges = true;
            var isvalid = EditContext.Validate();
            HasValidationErrors = !isvalid;

            if (OnFieldChanged.HasDelegate)
            {
                var arg = new FormFieldChangedEventArgs(isvalid, e.FieldIdentifier);
                OnFieldChanged.InvokeAsync(arg);
            }

            StateHasChanged();
        }

        private void RemoveEditContextHandler()
        {
            if (_hasChangeTracker && EditContext != null)
            {
                EditContext.OnFieldChanged -= HandleFieldChanged;
                _hasChangeTracker = false;
            }
        } 

        #endregion
    }
}
