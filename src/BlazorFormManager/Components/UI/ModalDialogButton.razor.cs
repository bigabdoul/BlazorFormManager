using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;

namespace BlazorFormManager.Components.UI
{
    /// <summary>
    /// Represents the base class for modal buttons.
    /// </summary>
    public abstract class ModalDialogButtonBase : ComponentBase
    {
        /// <summary>
        /// Gets or sets the target CSS selector for the modal button or link.
        /// </summary>
        [Parameter] public string? Target { get; set; }

        /// <summary>
        /// Gets or sets the button text.
        /// Will be ignored if <see cref="ChildContent"/> is specified.
        /// </summary>
        [Parameter] public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the icon for the button or link.
        /// Will be ignored if <see cref="ChildContent"/> is specified.
        /// </summary>
        [Parameter] public string? Icon { get; set; }

        /// <summary>
        /// Gets or sets the href attribute value for an anchor.
        /// </summary>
        [Parameter] public string? LinkUrl { get; set; } = "#";

        /// <summary>
        /// Gets or sets the button or link's CSS class.
        /// </summary>
        [Parameter] public string? CssClass { get; set; }

        /// <summary>
        /// Indicates whether the button or link is dismissable (data-bs-dismiss="modal").
        /// </summary>
        [Parameter] public bool Dismissable { get; set; }

        /// <summary>
        /// Gets or sets the child content render fragment.
        /// </summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Gets or sets the click event callback.
        /// </summary>
        [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

        /// <summary>
        /// Gets or sets the additional attributes dictionary.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IDictionary<string, object>? AdditionalAttributes { get; set; }

        /// <summary>
        /// When overriden, indicates whether the current component instance is a link.
        /// </summary>
        protected virtual bool IsLink { get; }

        /// <inheritdoc/>
		protected override void OnParametersSet()
		{
            if (!IsLink && CssClass == null)
			{
                CssClass = "btn btn-primary";
			}
			base.OnParametersSet();
		}
	}
}
