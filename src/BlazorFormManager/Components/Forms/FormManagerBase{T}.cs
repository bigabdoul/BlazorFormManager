using Microsoft.AspNetCore.Components;

namespace BlazorFormManager.Components.Forms
{
    /// <summary>
    /// Implements a strongly-typed <see cref="FormManagerBase"/> class.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class FormManagerBase<TModel> : FormManagerBase
    {
        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FormManagerBase"/> class.
        /// </summary>
        protected FormManagerBase() : base()
        {
        }

        #endregion

        #region properties

        #region parameters

        /// <summary>
        /// Gets or sets the form model to upload.
        /// </summary>
        [Parameter] public new TModel? Model
        {
            get => (TModel?)base.Model;
            set
            {
                if (!Equals(value, base.Model))
                {
                    base.Model = value;
                    SetEditContext();
                    NotifyModelChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets an event callback delegate used to notify about model changes.
        /// </summary>
        [Parameter] public EventCallback<TModel?> OnModelChanged { get; set; }

        #endregion

        #endregion

        #region methods

        /// <summary>
        /// Invokes the <see cref="OnModelChanged"/> if it has a delegate.
        /// </summary>
        protected virtual void NotifyModelChanged()
        {
            if (OnModelChanged.HasDelegate)
                OnModelChanged.InvokeAsync(Model);
        }

        /// <summary>
        /// Returns the <see cref="Model"/> property value.
        /// </summary>
        /// <returns></returns>
        public override object? GetModel() => Model;

        #endregion
    }
}
