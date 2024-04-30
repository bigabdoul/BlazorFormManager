#if !NETSTANDARD2_0 // IJSObjectReference is not available in netstandard2.0
using Carfamsoft.Model2View.Shared.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFormManager.DOM
{
    /// <summary>
    /// Dynamically loads a JavaScript module.
    /// </summary>
    public partial class JSModuleLoader : ComponentBase, IAsyncDisposable
    {
        /// <summary>
        /// Gets or sets the fully-qualified module path, optionally including the file extension.
        /// Do not set this property and <see cref="Sources"/> at the same time.
        /// </summary>
        [Parameter] public string? Source { get; set; }

        /// <summary>
        /// Gets or sets the fully-qualified module paths, optionally including the file extensions.
        /// If set, these modules are loaded in sequential order as appearing in a document.
        /// Do not set this property and <see cref="Source"/> at the same time.
        /// </summary>
        [Parameter] public IEnumerable<string>? Sources { get; set; }

        /// <summary>
        /// Gets or sets the default function to invoke after importing the module.
        /// </summary>
        [Parameter] public string? Initializer { get; set; }

        /// <summary>
        /// Gets or sets the default zero-based index of the module to invoke the <see cref="Initializer"/> function on.
        /// </summary>
        [Parameter] public int InitializerIndex { get; set; }

        /// <summary>
        /// Gets or sets the initialization options for JavaScript modules to load.
        /// </summary>
        [Parameter] public IList<ModuleLoaderOptions>? Options { get; set; }

        /// <summary>
        /// Gets or sets an array of objects to use as arguments for the <see cref="Initializer"/> function.
        /// </summary>
        [Parameter] public object?[]? InitializerArgs { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of attempts to invoke a 
        /// JavaScript function. Defaults to 4.
        /// </summary>
        [Parameter] public byte MaxAttempts { get; set; } = 4;

        /// <summary>
        /// Gets or sets the number of milliseconds to wait before retrying 
        /// a failed JavaScript function invocation. Defaults to 250.
        /// </summary>
        [Parameter] public ushort MillisecondsDelay { get; set; } = 250;

        /// <summary>
        /// Determines whether a ".js" file name extension should be 
        /// omitted from the <see cref="Source"/> if it has no extension.
        /// </summary>
        [Parameter] public bool NoAutoExtension { get; set; }

        /// <summary>
        /// Gets or sets the event callback when the specified module is loaded.
        /// </summary>
        [Parameter] public EventCallback<JSModuleLoader> OnModuleLoaded { get; set; }

        /// <summary>
        /// Gets or sets the event callback when a JavaScript object reference is loaded.
        /// </summary>
        [Parameter] public EventCallback<IJSObjectReference> OnModuleReferenceLoaded { get; set; }

        [Inject] IJSRuntime JsRuntime { get; set; } = null!;

        private readonly IList<IJSObjectReference> _modules = new List<IJSObjectReference>();
        private bool _disposed;

        /// <summary>
        /// Gets the module referencing <see cref="Source"/>.
        /// </summary>
        public IJSObjectReference? Module => _modules[0];

        /// <summary>
        /// Gets the collection of loaded modules.
        /// </summary>
        public IEnumerable<IJSObjectReference> Modules => _modules.ToArray();

        /// <summary>
        /// Invokes the <see cref="Initializer"/> function asynchronously.
        /// </summary>
        /// <param name="args">JSON-serializable arguments.</param>
        /// <returns></returns>
        public virtual ValueTask InvokeVoidAsync(params object?[]? args) => InvokeVoidAsync(Initializer!, args?.Length > 0 ? args : InitializerArgs);

        /// <summary>
        /// Invokes the <see cref="Initializer"/> function asynchronously in the module identified by <paramref name="moduleIndex"/>.
        /// </summary>
        /// <param name="moduleIndex">The zero-based index of the module to invoke the function on.</param>
        /// <param name="args">JSON-serializable arguments.</param>
        /// <returns></returns>
        public virtual ValueTask InvokeVoidAsync(int moduleIndex, params object?[]? args) => InvokeVoidAsync(Initializer!, moduleIndex, args?.Length > 0 ? args : InitializerArgs);

        /// <summary>
        /// Invokes the specified JavaScript function name asynchronously.
        /// </summary>
        /// <param name="identifier">An identifier for the function to invoke.</param>
        /// <param name="args">JSON-serializable arguments.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// The module identified by <see cref="Source"/> has not been loaded.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is null or blank.</exception>
        public virtual ValueTask InvokeVoidAsync(string identifier, params object?[]? args)
            => InvokeVoidAsync(identifier, InitializerIndex, args);

        /// <summary>
        /// Invokes the specified JavaScript function name asynchronously in the module identified by <paramref name="moduleIndex"/>.
        /// </summary>
        /// <param name="identifier">An identifier for the function to invoke.</param>
        /// <param name="moduleIndex">The zero-based index of the module to invoke the function on.</param>
        /// <param name="args">JSON-serializable arguments.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// The module identified by <see cref="Source"/> has not been loaded.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is null or blank.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="moduleIndex"/> is outside the bounds of loaded modules.</exception>
        public virtual ValueTask InvokeVoidAsync(string identifier, int moduleIndex, params object?[]? args)
        {
            EnsureValidOperation(identifier, moduleIndex);
            return InvokeVoidCoreAsync(_modules[moduleIndex], identifier, args);
        }

        /// <summary>
        /// Invokes the specified JavaScript function name asynchronously in the module identified by <paramref name="module"/>.
        /// </summary>
        /// <param name="module">The module on which to invoke the function.</param>
        /// <param name="identifier">An identifier for the function to invoke.</param>
        /// <param name="args">JSON-serializable arguments.</param>
        /// <returns></returns>
        protected virtual async ValueTask InvokeVoidCoreAsync(IJSObjectReference module, string identifier, params object?[]? args)
        {
            byte count = 0;
            while (++count <= MaxAttempts)
            {
                try
                {
                    await module.InvokeVoidAsync(identifier, args);
                    break;
                }
                catch (JSException)
                {
                    if (count == MaxAttempts) throw;
                    if (MillisecondsDelay > 0) await Task.Delay(MillisecondsDelay);
                }
            }
        }

        /// <summary>
        /// Invokes the specified JavaScript function asynchronously.
        /// </summary>
        /// <typeparam name="TValue">The JSON-serializable return type.</typeparam>
        /// <param name="args">JSON-serializable arguments.</param>
        /// <returns>An instance of <typeparamref name="TValue"/> obtained by JSON-deserializing the return value.</returns>
        public virtual ValueTask<TValue> InvokeAsync<TValue>(params object?[]? args)
            => InvokeAsync<TValue>(Initializer!, args);

        /// <summary>
        /// Invokes the specified JavaScript function asynchronously.
        /// </summary>
        /// <typeparam name="TValue">The JSON-serializable return type.</typeparam>
        /// <param name="identifier">An identifier for the function to invoke.</param>
        /// <param name="args">JSON-serializable arguments.</param>
        /// <returns>An instance of <typeparamref name="TValue"/> obtained by JSON-deserializing the return value.</returns>
        public virtual ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object?[]? args)
            => InvokeAsync<TValue>(identifier, InitializerIndex, args);

        /// <summary>
        /// Invokes the specified JavaScript function asynchronously in the module identified by <paramref name="moduleIndex"/>.
        /// </summary>
        /// <typeparam name="TValue">The JSON-serializable return type.</typeparam>
        /// <param name="identifier">An identifier for the function to invoke.</param>
        /// <param name="moduleIndex">The zero-based index of the module to invoke the function on.</param>
        /// <param name="args">JSON-serializable arguments.</param>
        /// <returns>An instance of <typeparamref name="TValue"/> obtained by JSON-deserializing the return value.</returns>
        /// <exception cref="IndexOutOfRangeException"><paramref name="moduleIndex"/> is outside the bounds of loaded modules.</exception>
        public virtual ValueTask<TValue> InvokeAsync<TValue>(string identifier, int moduleIndex, params object?[]? args)
        {
            EnsureValidOperation(identifier, moduleIndex);
            return InvokeCoreAsync<TValue>(_modules[moduleIndex], identifier, args);
        }

        /// <summary>
        /// Invokes the specified JavaScript function asynchronously in the module identified by <paramref name="module"/>.
        /// </summary>
        /// <typeparam name="TValue">The JSON-serializable return type.</typeparam>
        /// <param name="module">The module on which to invoke the function.</param>
        /// <param name="identifier">An identifier for the function to invoke.</param>
        /// <param name="args">JSON-serializable arguments.</param>
        /// <returns>An instance of <typeparamref name="TValue"/> obtained by JSON-deserializing the return value.</returns>
        protected virtual async ValueTask<TValue> InvokeCoreAsync<TValue>(IJSObjectReference module, string identifier, params object?[]? args)
        {
            TValue result = default!;
            byte count = 0, max = MaxAttempts;
            while (++count <= max)
            {
                try
                {
                    result = await module.InvokeAsync<TValue>(identifier, args);
                    break;
                }
                catch (JSException)
                {
                    if (count == max) throw;
                    if (MillisecondsDelay > 0) await Task.Delay(MillisecondsDelay);
                }
            }
            return result;
        }

        /// <inheritdoc/>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadAndInitializeModulesAsync();
            }
        }

        /// <summary>
        /// Imports and initializes all modules specified either by <see cref="Sources"/> or <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// Parameters <see cref="Source"/> and <see cref="Sources"/> cannot be set simultaneously.
        /// </exception>
        /// <exception cref="ObjectDisposedException">The component has been disposed.</exception>
        public async Task LoadAndInitializeModulesAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            var nonBlankSource = Source.IsNotBlank();
            var sourcesExist = Sources?.Any() == true;

            if (nonBlankSource && sourcesExist)
                // avoid confusion
                throw new InvalidOperationException($"Parameters {nameof(Source)} and {nameof(Sources)} cannot be set simultaneously.");

            if (Options != null)
            {
                if (nonBlankSource || sourcesExist)
                    throw new InvalidOperationException($"Parameters {nameof(Source)} or {nameof(Sources)} " +
                        $"cannot be set when {nameof(Options)} is specified.");

                await DisposeModulesAsync();

                foreach (var option in Options)
                {
                    option.Module = await LoadModuleAsync(option.Source);
                    if (option.HotInit && option.HasInitializer)
                        await InvokeVoidCoreAsync(option.Module, option.Identifier!, option.Args);
                }

                foreach (var option in Options.Where(o => !o.HotInit && o.HasInitializer))
                    await InvokeVoidCoreAsync(option.Module!, option.Identifier!, option.Args);

                if (OnModuleLoaded.HasDelegate)
                    await OnModuleLoaded.InvokeAsync(this);
            }
            else if (nonBlankSource || sourcesExist)
            {
                var paths = nonBlankSource ? new[] { Source! } : Sources!;

                if (!paths.Any()) return;

                await DisposeModulesAsync();

                foreach (var name in paths)
                    _ = await LoadModuleAsync(name);

                if (OnModuleLoaded.HasDelegate)
                    await OnModuleLoaded.InvokeAsync(this);

                if (Initializer.IsNotBlank())
                    await InvokeVoidAsync(Initializer!, InitializerIndex, InitializerArgs);

            }

            async Task<IJSObjectReference> LoadModuleAsync(string path)
            {
                if (path.IsBlank())
                    throw new InvalidOperationException("The source of the module to load is required.");
                
                if (!NoAutoExtension && System.IO.Path.GetExtension(path!).IsBlank())
                    path += ".js";

                var module = await JsRuntime.InvokeAsync<IJSObjectReference>("import", path);
                _modules.Add(module);

                if (OnModuleReferenceLoaded.HasDelegate)
                    await OnModuleReferenceLoaded.InvokeAsync(module);

                return module;
            }
        }

        /// <summary>
        /// Verifies this <see cref="JSModuleLoader"/> has not been disposed,
        /// the module is not null, and the identifier is not blank before 
        /// invoking the specified JavaScript function <paramref name="identifier"/>.
        /// </summary>
        /// <param name="identifier">An identifier for the function to invoke.</param>
        /// <exception cref="ObjectDisposedException">This <see cref="JSModuleLoader"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">The module has not been loaded.</exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="identifier"/> is null, empty, or contains only whitespace characters.
        /// </exception>
        protected virtual void EnsureValidOperation(string identifier)
            => EnsureValidOperation(identifier, InitializerIndex);

        /// <summary>
        /// Verifies this <see cref="JSModuleLoader"/> has not been disposed,
        /// the module is not null, and the identifier is not blank before 
        /// invoking the specified JavaScript function <paramref name="identifier"/>.
        /// </summary>
        /// <param name="identifier">An identifier for the function to invoke.</param>
        /// <param name="moduleIndex">The zero-based index of the module to invoke the function on.</param>
        /// <exception cref="ObjectDisposedException">This <see cref="JSModuleLoader"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">The module has not been loaded.</exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="identifier"/> is null, empty, or contains only whitespace characters.
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// <paramref name="moduleIndex"/> is outside the bounds of loaded modules.
        /// </exception>
        protected virtual void EnsureValidOperation(string identifier, int moduleIndex)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (identifier.IsBlank())
                throw new ArgumentNullException(nameof(identifier));

            if (_modules.Count == 0)
                throw new InvalidOperationException($"No module referenced by {Source ?? nameof(Source)} or " +
                    $"the {nameof(Sources)} collection has been loaded.");

            if (moduleIndex < 0 || moduleIndex > _modules.Count - 1)
                throw new IndexOutOfRangeException("The module index is outside the bounds of loaded modules.");
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!_disposed)
            {
                await DisposeModulesAsync();
                _disposed = true;
            }
        }

        async Task DisposeModulesAsync()
        {
            foreach (var module in _modules)
                try { await module.DisposeAsync(); } catch { }
            _modules.Clear();
        }
    }

    /// <summary>
    /// Encapsulates initialization configuration for a JavaScript module.
    /// </summary>
    public sealed class ModuleLoaderOptions
    {
        /// <summary>
        /// Gets or sets the source of the module to load.
        /// </summary>
        public string Source { get; set; } = default!;

        /// <summary>
        /// Gets or sets the function identifier for initialization purposes.
        /// </summary>
        public string Identifier { get; set; } = default!;

        /// <summary>
        /// Gets or sets the arguments to pass to the initialization function.
        /// </summary>
        public object?[]? Args { get; set; }

        /// <summary>
        /// When true, instructs the module loader to run the initialization 
        /// function as soon as the referenced source module is loaded.
        /// </summary>
        public bool HotInit { get; set; }

        /// <summary>
        /// Gets or sets the loaded module associated with this instance.
        /// </summary>
        internal IJSObjectReference? Module { get; set; }

        /// <summary>
        /// Indicates whether this instance has a loaded module and an initialization function.
        /// </summary>
        public bool HasInitializer => Module != null && !string.IsNullOrWhiteSpace(Identifier);
    }
}
#endif
