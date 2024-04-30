    #if NET6_0_OR_GREATER
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Carfamsoft.JSInterop;

/// <summary>
/// Loads a JavaScript module on demand when first needed that can be used to invoke functions.
/// </summary>
/// <param name="jsRuntime">The JavaScript runtime provider.</param>
public class LazyJsModuleLoader(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private bool _disposed;
    private Lazy<Task<IJSObjectReference>>? _moduleTask;

    /// <summary>
    /// Gets or sets the path to the JavaScript module to load.
    /// </summary>
    public string ModuleSource { get; set; } = default!;

    /// <summary>
    /// Gets the task used to lazily load the module referenced by <see cref="ModuleSource"/>.
    /// </summary>
    public virtual Lazy<Task<IJSObjectReference>> ModuleTask
    {
        get
        {
            var path = ModuleSource ?? throw new InvalidOperationException($"{nameof(ModuleSource)} is not defined.");
            _moduleTask ??= new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", path).AsTask());
            return _moduleTask;
        }
    }

    /// <summary>
    /// Invokes the specified JavaScript function asynchronously.
    /// </summary>
    /// <param name="identifier">An identifier for the function to invoke.</param>
    /// <param name="args">JSON-serializable arguments.</param>
    /// <returns></returns>
    public async ValueTask InvokeVoidAsync(string identifier, params object?[]? args)
    {
        var module = await ModuleTask.Value;
        await module.InvokeVoidAsync(identifier, args);
    }

    /// <summary>
    /// Invokes the specified JavaScript function asynchronously.
    /// </summary>
    /// <typeparam name="TValue">The JSON-serializable return type.</typeparam>
    /// <param name="identifier">An identifier for the function to invoke.</param>
    /// <param name="args">JSON-serializable arguments.</param>
    /// <returns>An instance of <typeparamref name="TValue"/> obtained by JSON-deserializing the return value.</returns>
    public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object?[]? args)
    {
        var module = await ModuleTask.Value;
        var result = await module.InvokeAsync<TValue>(identifier, args);
        return result;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the underlying module if it has been loaded.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to dispose managed resources; otherwise, <see langword="false"/>.</param>
    /// <returns></returns>
    protected virtual async Task DisposeAsync(bool disposing)
    {
        if (disposing && !_disposed)
        {
            try
            {
                if (ModuleTask.IsValueCreated)
                {
                    var module = await ModuleTask.Value;
                    await module.DisposeAsync();
                    _moduleTask = null;
                }
            }
            catch (InvalidOperationException)
            {
            }
            _disposed = true;
        }
    }
}
#endif