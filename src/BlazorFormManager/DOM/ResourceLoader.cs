#if !NETSTANDARD2_0
using BlazorFormManager.Components.Forms;
using Carfamsoft.JSInterop.Extensions;
using Carfamsoft.Model2View.Shared.Extensions;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFormManager.DOM
{
    /// <summary>
    /// Loads JavaScript resources.
    /// </summary>
    public class ResourceLoader : FormManagerBase
    {
        /// <summary>
        /// Gets or sets the script to load.
        /// </summary>
        [Parameter] public string? Script { get; set; }

        /// <summary>
        /// Gets or sets the style sheet to load.
        /// </summary>
        [Parameter] public string? Style { get; set; }

        /// <summary>
        /// Gets or sets a collection of scripts to load.
        /// </summary>
        [Parameter] public IEnumerable<string>? Scripts { get; set; }

        /// <summary>
        /// Gets or sets a collection of style sheets to load.
        /// </summary>
        [Parameter] public IEnumerable<string>? Styles { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of attempts to invoke a 
        /// JavaScript function. Defaults to 10.
        /// </summary>
        [Parameter] public int MaxAttempts { get; set; } = 10;

        /// <summary>
        /// Gets or sets the number of milliseconds to wait before 
        /// retrying a JavaScript function invocation. Defaults to 1000.
        /// </summary>
        [Parameter] public int MillisecondsDelay { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the async property on the script tag to load.
        /// </summary>
        [Parameter] public bool IsAsync { get; set; }

        /// <summary>
        /// Gets or sets the deferred property on the script tag to load.
        /// </summary>
        [Parameter] public bool IsDeferred { get; set; }

        /// <summary>
        /// Gets or sets the event callback delegate to invoke when all styles have been loaded successfully.
        /// </summary>
        [Parameter] public EventCallback OnStylesLoaded { get; set; }

        /// <summary>
        /// Gets or sets the event callback delegate to invoke when all scripts have been loaded successfully.
        /// </summary>
        [Parameter] public EventCallback OnScriptsLoaded { get; set; }

        private bool _busy;
        private bool _initialized;
        private EventCallback<FormManagerBase>? scriptInitializedCallback;
        private const string INSERT_SCRIPTS = BlazorFormManagerNS + ".insertDomScripts";
        private const string INSERT_STYLES = BlazorFormManagerNS + ".insertDomStyles";

        /// <summary>
        /// Returns null.
        /// </summary>
        /// <returns></returns>
        public override object? GetModel() => null;

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            //scriptInitializedCallback = OnAfterScriptInitialized;

            OnAfterScriptInitialized = EventCallback.Factory.Create<FormManagerBase>(this, async form =>
            {
                // initialize script sources, if any
                await LoadResourcesAsync();

                if (scriptInitializedCallback.HasValue && scriptInitializedCallback.Value.HasDelegate)
                {
                    await scriptInitializedCallback.Value.InvokeAsync(form);
                }
            });
        }

        /// <inheritdoc/>
        protected override async Task OnParametersSetAsync()
        {
            if ( ! scriptInitializedCallback.HasValue && OnAfterScriptInitialized.HasDelegate)
                scriptInitializedCallback = OnAfterScriptInitialized;

            if (!_initialized && !_busy)
            {
                await LoadResourcesAsync();
            }
        }

        /// <summary>
        /// Loads all styles and scripts asynchronously.
        /// </summary>
        /// <returns></returns>
        public virtual async ValueTask LoadResourcesAsync()
        {
            if (_initialized || _busy) return;
            _busy = true;
            try
            {
                List<string> resources = new();

                if (Style.IsNotBlank()) resources.Add(Style!);
                if (Styles?.Any() == true) resources.AddRange(Styles.Where(s => s.IsNotBlank()));

                if (resources.Count != 0)
                {
                    var success = await JS.SafeInvokeAsync<bool>(MaxAttempts, MillisecondsDelay, INSERT_STYLES, FormId, resources.ToArray());
                    if (success && OnStylesLoaded.HasDelegate)
                        await OnStylesLoaded.InvokeAsync(this);
                    resources.Clear();
                }

                if (Script.IsNotBlank()) resources.Add(Script!);
                if (Scripts?.Any() == true) resources.AddRange(Scripts.Where(s => s.IsNotBlank()));

                if (resources.Count != 0)
                {
                    var success = await JS.SafeInvokeAsync<bool>(MaxAttempts, MillisecondsDelay, INSERT_SCRIPTS, FormId, resources.ToArray(), IsAsync, IsDeferred);
                    if (success && OnScriptsLoaded.HasDelegate)
                        await OnScriptsLoaded.InvokeAsync(this);
                    resources.Clear();
                }

                _initialized = true;
            }
            finally
            {
                _busy = false;
            }
        }
    }

}
#endif