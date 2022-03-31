using Carfamsoft.Model2View.Shared.Extensions;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using CollectionExtensions = Carfamsoft.Model2View.Shared.Collections.CollectionExtensions;

namespace BlazorFormManager.Components.UI
{
    /// <summary>
    /// Represents a Twitter Bootstrap (v5) Modal Component.
    /// </summary>
    public partial class ModalDialog
    {
		#region	parameters

		/// <summary>
		/// Gets or sets the modal identifier.
		/// </summary>
		[Parameter] public string Id { get; set; } = typeof(ModalDialog).Name.GenerateId(camelCase: true);

		/// <summary>
		/// Gets or sets the modal dialog title render fragment.
		/// </summary>
		[Parameter] public RenderFragment? DialogTitle { get; set; }

		/// <summary>
		/// Gets or sets the modal dialog content render fragment.
		/// </summary>
		[Parameter] public RenderFragment? DialogContent { get; set; }

		/// <summary>
		/// Gets or sets the modal dialog custom actions render fragment.
		/// This fragment is rendered in the modal footer.
		/// </summary>
		[Parameter] public RenderFragment? DialogActions { get; set; }

		/// <summary>
		/// Gets or sets the modal dialog size. Support sizes are <see cref="ComponentSize.Sm"/>,
		/// <see cref="ComponentSize.Lg"/>, and <see cref="ComponentSize.Xl"/>.
		/// </summary>
		[Parameter] public ComponentSize Size { get; set; }

		/// <summary>
		/// Gets or sets the modal's close button text.
		/// </summary>
		[Parameter] public string? CloseButtonText { get; set; }

		/// <summary>
		/// Gets or sets the modal's close button icon.
		/// </summary>
		[Parameter] public string? CloseButtonIcon { get; set; }

		/// <summary>
		/// Gets or sets the modal's close button click event callback.
		/// </summary>
		[Parameter] public EventCallback OnCloseClicked { get; set; }

		/// <summary>
		/// Indicates whether the modal should fade.
		/// </summary>
		[Parameter] public bool Fade { get; set; }

		/// <summary>
		/// Excludes a modal-backdrop element.
		/// </summary>
		[Parameter] public bool NoBackdrop { get; set; }

		/// <summary>
		/// Includes a backdrop which doesn't close the modal on click.
		/// </summary>
		[Parameter] public bool StaticBackdrop { get; set; }

		/// <summary>
		/// Puts the focus on the modal when initialized.
		/// </summary>
		[Parameter] public bool Focus { get; set; }

		/// <summary>
		/// Closes the modal when escape key is pressed.
		/// </summary>
		[Parameter] public bool KeyboardEscape { get; set; }

		/// <summary>
		/// Scrolls independently of the page itself when the content 
		/// becomes too long for the user's viewport or device.
		/// </summary>
		[Parameter] public bool Scrollable { get; set; }

		/// <summary>
		/// Vertically centers the modal.
		/// </summary>
		[Parameter] public bool CenterVertical { get; set; }

		/// <summary>
		/// Whether to center the modal's header title.
		/// </summary>
		[Parameter] public bool CenterHeaderTitle { get; set; }

		/// <summary>
		/// Wraps the modal's body within .container-fluid CSS selector.
		/// Useful when intended to use the normal grid system classes.
		/// </summary>
		[Parameter] public bool FluidContainer { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Parameter] public bool Fullscreen { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Parameter] public ComponentSize FullscreenBelowSize { get; set; }

		/// <summary>
		/// Hides or shows the top close button. If <see cref="CenterHeaderTitle" /> is true,
		/// the button won't show up unless <see cref="ForceTopClose" /> is true.
		/// </summary>
		[Parameter] public bool HideTopClose { get; set; }

		/// <summary>
		/// Whether to force show the top close button regardless of <see cref="CenterHeaderTitle" /> being true.
		/// This could lead to the title not being centered due to the default Twitter Bootstrap CSS settings
		/// (display: flex; justify-content: space-between) on the .modal-header CSS selector.
		/// </summary>
		[Parameter] public bool ForceTopClose { get; set; }

		#endregion

		#region fields

		private readonly string _ariaLabelledBy = $"modalDialogTitle_{typeof(ModalDialog).Name.GenerateId()}";

		#endregion

		#region helpers

		/// <summary>
		/// Gets the modal dialog's attributes.
		/// </summary>
		protected virtual IDictionary<string, object> GetModalAttributes() => CollectionExtensions.GetAttributes
		(
			("id", Id),
			("tabindex", -1),
			("aria-hidden", "true"),
			("aria-labelledby", _ariaLabelledBy),
			("data-bs-focus", Focus ? "true" : string.Empty),
			("data-bs-keyboard", KeyboardEscape ? string.Empty : "false"),
			("data-bs-backdrop", StaticBackdrop ? "static" : (NoBackdrop ? "false" : string.Empty))
		);

		/// <summary>
		/// Indicates whether the top close button is visible.
		/// </summary>
		protected virtual bool TopCloseButtonVisible => !HideTopClose && (!CenterHeaderTitle || ForceTopClose);

		/// <summary>
		/// Indicates whether the footer close button is visible.
		/// </summary>
		protected virtual bool FooterCloseButtonVisible 
			=> !string.IsNullOrWhiteSpace(CloseButtonText) || OnCloseClicked.HasDelegate;

		/// <summary>
		/// Gets the modal CSS class.
		/// </summary>
		protected virtual string ModalClass => "modal" + (Fade ? " fade" : string.Empty);

		/// <summary>
		/// Gets the modal dialog's CSS class.
		/// </summary>
		protected virtual string ModalDialogClass 
			=> $"modal-dialog{ModalSizeClass}{FullscreenSizeClass}{ScrollableClass}{CenterVerticalClass}";

		/// <summary>
		/// Gets the modal header's CSS class.
		/// </summary>
		protected virtual string ModalHeaderClass 
			=> "modal-header" + (CenterHeaderTitle ? " justify-content-center" : string.Empty);

		/// <summary>
		/// Gets the CSS class for a scrollable modal.
		/// </summary>
		protected virtual string ScrollableClass => Scrollable ? " modal-dialog-scrollable" : string.Empty;

		/// <summary>
		/// Gets the CSS class for a centered modal.
		/// </summary>
		protected virtual string CenterVerticalClass 
			=> !Fullscreen && CenterVertical ? " modal-dialog-centered" : string.Empty;

		/// <summary>
		/// If not in fullscreen mode, gets the CSS class that controls the modal's size.
		/// </summary>
		protected virtual string ModalSizeClass
		{
			get
			{
				if (Fullscreen) return string.Empty;
				return Size switch
				{
					ComponentSize.None or ComponentSize.Md => string.Empty,
					ComponentSize.Xl or ComponentSize.Xxl => " modal-xl",
					_ => $" modal-{Size.ToString().ToLower()}",
				};
			}
		}

		/// <summary>
		/// If in fullscreen mode, gets the CSS class that determines the break-point size.
		/// </summary>
		protected virtual string FullscreenSizeClass
		{
			get
			{
				const string modal_fullscreen = " modal-fullscreen";
				if (!Fullscreen) return string.Empty;
				return FullscreenBelowSize switch
				{
					ComponentSize.None => modal_fullscreen,
					_ => $" {modal_fullscreen}-{FullscreenBelowSize.ToString().ToLower()}-down",
				};
			}
		}

		#endregion
	}
}
