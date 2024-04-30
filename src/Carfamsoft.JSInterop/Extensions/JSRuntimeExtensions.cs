using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Carfamsoft.JSInterop.Extensions
{
    /// <summary>
    /// Provides extension methods for class instances that implement the <see cref="IJSRuntime"/> interface.
    /// </summary>
    public static class JSRuntimeExtensions
    {
        /// <summary>
        /// Safely invokes the specified JavaScript function asynchronously.
        /// </summary>
        /// <param name="jSRuntime">The <see cref="IJSRuntime"/>.</param>
        /// <param name="maxAttempts">The maximum number of initialization attempts.</param>
        /// <param name="millisecondsDelay">
        /// The number of milliseconds to wait before completing the returned task, or -1 to wait indefinitely.
        /// </param>
        /// <param name="identifier">An identifier for the function to invoke.</param>
        /// <param name="args">JSON-serializable arguments.</param>
        /// <returns>A task that represents the the asynchronous invocation operation.</returns>
        public static async Task SafeInvokeVoidAsync(this IJSRuntime jSRuntime, int maxAttempts, int millisecondsDelay, string identifier, params object?[]? args)
        {
            var initCount = 0;
            while (true)
            {
                try
                {
                    await jSRuntime.InvokeVoidAsync(identifier, args);
                    break;
                }
                catch
                {
                    if (++initCount >= maxAttempts)
                    {
                        throw;
                    }
                    await Task.Delay(millisecondsDelay);
                }
            }
        }

        /// <summary>
        /// Safely invokes the specified JavaScript function asynchronously.
        /// </summary>
        /// <typeparam name="TValue">The JSON-serializable return type.</typeparam>
        /// <param name="jSRuntime">The <see cref="IJSRuntime"/>.</param>
        /// <param name="maxAttempts">The maximum number of initialization attempts.</param>
        /// <param name="millisecondsDelay">
        /// The number of milliseconds to wait before completing the returned task, or -1 to wait indefinitely.
        /// </param>
        /// <param name="identifier">An identifier for the function to invoke.</param>
        /// <param name="args">JSON-serializable arguments.</param>
        /// <returns></returns>
        public static async ValueTask<TValue> SafeInvokeAsync<TValue>(this IJSRuntime jSRuntime, int maxAttempts, int millisecondsDelay, string identifier, params object?[]? args)
        {
            TValue result;
            var initCount = 0;
            while (true)
            {
                try
                {
                    result = await jSRuntime.InvokeAsync<TValue>(identifier, args);
                    break;
                }
                catch
                {
                    if (++initCount >= maxAttempts)
                    {
                        throw;
                    }
                    await Task.Delay(millisecondsDelay);
                }
            }
            return result;
        }
    }
}
