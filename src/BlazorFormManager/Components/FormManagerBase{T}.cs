using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
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
        private bool _parametersSet;
        private readonly EventHandler<FieldChangedEventArgs> _fieldChangedHandler;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="FormManagerBase"/> class.
        /// </summary>
        protected FormManagerBase()
        {
            _fieldChangedHandler = HandleFieldChanged;
        }

        #region properties

        #region protected

        /// <summary>
        /// Gets or sets the form's current edit context.
        /// </summary>
        protected EditContext EditContext { get; private set; }

        /// <summary>
        /// Indicates whether the form has unsaved changes.
        /// </summary>
        public bool HasChanges { get; protected set; }

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
        /// Gets or sets an event callback delegate used to notify about model changes.
        /// </summary>
        [Parameter] public EventCallback<TModel> OnModelChanged { get; set; }

        #endregion

        #endregion

        #region methods

        /// <summary>
        /// Invokes the <see cref="OnModelChanged"/> if it has a delegate.
        /// </summary>
        protected virtual void NotifyModelChanged()
        {
            if (OnModelChanged.HasDelegate)
                OnModelChanged.InvokeAsync(_model);
        }

        /// <summary>
        /// Returns the <see cref="Model"/> property value.
        /// </summary>
        /// <returns></returns>
        public override object GetModel() => Model;

        /// <summary>
        /// Attaches a handler to the <see cref="EditContext.OnFieldChanged"/> event.
        /// </summary>
        protected virtual void AttachFieldChangedListener()
        {
            if (EditContext != null)
            {
                EditContext.OnFieldChanged += _fieldChangedHandler;
            }
        }

        /// <summary>
        /// Detaches a handler to the <see cref="EditContext.OnFieldChanged"/> event.
        /// </summary>
        protected virtual void DetachFieldChangedListener()
        {
            if (EditContext != null)
            {
                EditContext.OnFieldChanged -= _fieldChangedHandler;
            }
        }

        /// <inheritdoc/>
        protected override Task HandleSubmitDoneAsync(FormManagerSubmitResult result)
        {
            HasChanges = !result.Succeeded;
            return base.HandleSubmitDoneAsync(result);
        }

        /// <inheritdoc/>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            _parametersSet = true;
            if (EditContext == null) SetEditContext();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            DetachFieldChangedListener();
            base.Dispose(disposing);
        }

        #endregion

        #region helpers

        private void SetEditContext()
        {
            if (!_parametersSet) return;
            
            DetachFieldChangedListener();
            NotifyModelChanged();

            if (Equals(_model, default))
            {
                EditContext = null;
                return;
            }

            EditContext = new EditContext(_model);
            AttachFieldChangedListener();
                
            StateHasChanged();
        }

        private void HandleFieldChanged(object sender, FieldChangedEventArgs e)
        {
            HasChanges = true;

            if (EnableChangeTracking)
            {
                bool isValid;

                if (ValidateOnFieldChanged)
                {
                    isValid = EditContext.Validate();
                    HasValidationErrors = !isValid;
                }
                else isValid = false;

                if (OnFieldChanged.HasDelegate)
                {
                    OnFieldChanged.InvokeAsync(new FormFieldChangedEventArgs(isValid, e.FieldIdentifier));
                }
            }

            StateHasChanged();
        }

        #endregion
    }
}
