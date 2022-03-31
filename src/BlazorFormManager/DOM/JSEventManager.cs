using BlazorFormManager.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BlazorFormManager.DOM
{
    /// <summary>
    /// Provides static methods for managing JavaScript events through interoperability.
    /// </summary>
    public static class JSEventManager
    {
        #region fields

        private static readonly string Asm = typeof(JSEventManager).Assembly.GetName().Name!;

        private static readonly ConcurrentDictionary<string, Func<JSKeyboardEventArgs, Task>> _keyboardEvents = new();

        private static readonly ConcurrentDictionary<string, Func<JSEventArgs, Task>> _dictionary = new();

        #endregion

        /// <summary>
        /// Adds an event handler to the specified element identifier.
        /// </summary>
        /// <param name="js">An instance of a JavaScript runtime to which calls are dispatched.</param>
        /// <param name="targetId">The DOM element identifier to which to add the event listener.</param>
        /// <param name="eventType">The type of event to be added (e.g. 'keypress', 'load', 'input', etc.).</param>
        /// <param name="callback">An event handler delegate to invoke when the event is intercepted.</param>
        /// <returns></returns>
        public static async Task<bool> AddEventListenerAsync(this IJSRuntime js, string targetId, string eventType, Func<JSEventArgs, Task> callback)
        {
            var success = await js.InvokeAsync<bool>($"{Asm}.addEventListener", targetId, eventType, nameof(OnEventCallback));

            if (success)
                _dictionary.TryAdd($"{targetId}.{eventType}", callback);

            return success;
        }

        /// <summary>
        /// Blocks unwanted keyboard events.
        /// </summary>
        /// <param name="js">An instance of a JavaScript runtime to which calls are dispatched.</param>
        /// <param name="targetId">The DOM element identifier to which to add the event listener.</param>
        /// <param name="eventType">Either <see cref="DomEventType.KeyDown"/> or <see cref="DomEventType.KeyPress"/>.</param>
        /// <param name="callback">An event handler delegate to invoke when the event is intercepted.</param>
        /// <param name="filter">An object that specifies options for filtering keystrokes received from a keyboard.</param>
        /// <returns></returns>
        public static async Task<bool> FilterKeysAsync(this IJSRuntime js, string targetId, DomEventType eventType,
                                                       Func<JSKeyboardEventArgs, Task> callback,
                                                       FilterKeyOptions? filter = null)
        {
            switch (eventType)
            {
                case DomEventType.KeyDown:
                case DomEventType.KeyPress:
                case DomEventType.KeyUp:
                    var eventName = $"{eventType}".ToLower();
                    var options = new
                    {
                        targetId,
                        eventType = eventName,
                        callback = nameof(OnKeyboardEventCallback),
                        filter,
                    };

                    var success = await js.InvokeAsync<bool>($"{Asm}.filterKeys", options);

                    if (success)
                        _keyboardEvents.TryAdd($"{targetId}.{eventName}", callback);

                    return success;
                default:
                    throw new ArgumentException($"Only {nameof(DomEventType.KeyDown)} " +
                        $"and {nameof(DomEventType.KeyPress)}, and {nameof(DomEventType.KeyUp)} " +
                        "events are supported.");
            }
        }

        /// <summary>
        /// Removes a previously-registered event handler.
        /// </summary>
        /// <param name="js">An instance of a JavaScript runtime to which calls are dispatched.</param>
        /// <param name="targetId">The DOM element identifier for which to remove the event handler.</param>
        /// <param name="eventType">The event type to remove from the identified element.</param>
        /// <returns></returns>
        public static async Task RemoveEventListenerAsync(this IJSRuntime js, string targetId, string eventType)
        {
            var key = $"{targetId}.{eventType}";
            if (_keyboardEvents.TryRemove(key, out _) || _dictionary.TryRemove(key, out _))
                await js.InvokeVoidAsync($"{Asm}.removeEventListener", targetId, eventType);
        }

        /// <summary>
        /// Event handler delegate invoked when an event is intercepted in JavaScript.
        /// </summary>
        /// <param name="e">The event data.</param>
        /// <returns></returns>
        [JSInvokable]
        public static Task OnEventCallback(JSEventArgs e)
        {
            if (_dictionary.TryGetValue($"{e.TargetId}.{e.EventType}", out var callback))
                return callback.Invoke(e);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Event handler delegate invoked when a keyboard event is intercepted in JavaScript.
        /// </summary>
        /// <param name="e">The event data.</param>
        /// <returns></returns>
        [JSInvokable]
        public static Task OnKeyboardEventCallback(JSKeyboardEventArgs e)
        {
            if (_keyboardEvents.TryGetValue($"{e.TargetId}.{e.EventType}", out var callback))
                return callback.Invoke(e);
            return Task.CompletedTask;
        }
    }
}
