using BlazorFormManager.Components.Forms;
using Carfamsoft.JSInterop.Extensions;
using Carfamsoft.Model2View.Shared.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorFormManager.Components.UI
{
    /// <summary>
    /// Represents a rich text editor.
    /// </summary>
    public partial class QuillEditor : IDisposable
    {
        /// <summary>
        /// Gets or sets the CSS selector for the target element.
        /// </summary>
        [Parameter] public string? Target { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Parameter] public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        [Parameter] public string? Theme { get; set; } = "snow";

        /// <summary>
        /// Gets or sets a callback function invoked when the text changes.
        /// </summary>
        [Parameter] public EventCallback<string?> OnChange { get; set; }

        [CascadingParameter] private FormManagerBase? Form { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

        private DotNetObjectReference<QuillEditorJsInvokable>? dotNetObjRef;

        private bool alreadyRendered;
        private bool disposed;
        private readonly string EditorId = typeof(QuillEditor).Name.GenerateId(camelCase: true);
        private readonly string ToolbarId = typeof(QuillToolbar).Name.GenerateId(camelCase: true);
        internal readonly string ChangeToken = nameof(QuillEditor) + Guid.NewGuid().GetHashCode().ToString("x");

        /// <inheritdoc/>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !alreadyRendered)
            {
                if (OnChange.HasDelegate && dotNetObjRef is null)
                {
                    dotNetObjRef = DotNetObjectReference.Create(new QuillEditorJsInvokable(this));
                }

                var options = new
                {
                    formId = Form?.FormId,
                    theme = Theme,
                    dotNetObjectReference = dotNetObjRef,
                    onValueChanged = nameof(QuillEditorJsInvokable.OnValueChanged),
                    changeToken = ChangeToken,
                };

                var selector = Target.IsBlank() ? $"#{EditorId}" : Target!;
                await JSRuntime.SafeInvokeVoidAsync(10, 500, "BlazorFormManager.Quill.create", selector, options);
                alreadyRendered = true;
            }
        }

        internal Task OnValueChanged(string? value) => InvokeAsync(() => OnChange.InvokeAsync(value));

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing) 
        {
            if (disposing && disposed)
            {
                dotNetObjRef?.Dispose();
                disposed = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    internal sealed class QuillEditorJsInvokable
    {
        private readonly QuillEditor editor;

        public QuillEditorJsInvokable(QuillEditor editor)
        {
            this.editor = editor;
        }

        [JSInvokable]
        public async Task OnValueChanged(string? value, string changeToken)
        {
            if (changeToken == editor.ChangeToken)
                await editor.OnValueChanged(value);
            else
                Console.Error.WriteLine($"Invalid change token provided: {changeToken}.");
        }
    }
}
