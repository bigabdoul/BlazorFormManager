// Copyright (c) Karfamsoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.JSInterop;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Carfamsoft.JSInterop
{
    /// <summary>
    /// Represents an instance of a JavaScript localStorage to which calls may be dispatched.
    /// </summary>
    public class LocalStorage : ILocalStorage
    {
        #region fields

        private readonly IJSRuntime JS;
        private string? _authTokenKey;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes new instance of the <see cref="LocalStorage"/> class using 
        /// the specified <see cref="IJSRuntime"/>.
        /// </summary>
        /// <param name="jSRuntime">The JavaScript runtime instance to which calls are dispatched.</param>
        /// <exception cref="ArgumentNullException"><paramref name="jSRuntime"/> is null.</exception>
        public LocalStorage(IJSRuntime jSRuntime)
        {
            JS = jSRuntime ?? throw new ArgumentNullException(nameof(jSRuntime));
        }

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the globalThis module; can be the window object or 
        /// other supported environments. The default value is "globalThis".
        /// </summary>
        public virtual string GlobalThis { get; set; } = "globalThis";

        /// <summary>
        /// Gets or sets the application name.
        /// </summary>
        public virtual string? ApplicationName { get; set; }

        /// <summary>
        /// Gets the authorization token key name.
        /// </summary>
        public virtual string AuthorizationTokenKey
        {
            get => _authTokenKey ??= $"{ApplicationName}.AuthorizationToken";
            set
            {
                _authTokenKey = value;
            }
        }

        /// <summary>
        /// Gets the localStorage's qualifier format.
        /// </summary>
        private string LocalStorageIdFormat =>
            $"{GlobalThis}{(string.IsNullOrWhiteSpace(GlobalThis) ? string.Empty : ".")}" +
            "localStorage.{0}Item";

        #endregion

        #region methods

        /// <summary>
        /// Returns the specified key's value asynchronously.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        public async Task<string> GetItemAsync(string key)
        {
            return await JS.InvokeAsync<string>(string.Format(LocalStorageIdFormat, "get"), key);
        }

        /// <summary>
        /// Retrieves the specified key's value, deserializes and returns it asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the stored value.</typeparam>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="options">Options to control the behaviour during deserialization.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        public async Task<T?> GetItemAsync<T>(string key, JsonSerializerOptions? options = null)
        {
            var json = await GetItemAsync(key);
            if (json == null) return default;
            return JsonSerializer.Deserialize<T>(json, options);
        }

        /// <summary>
        /// Removes the key from the storage asynchronously.
        /// </summary>
        /// <param name="key">The key of the value to remove.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        public async Task RemoveItemAsync(string key)
        {
            await JS.InvokeVoidAsync(string.Format(LocalStorageIdFormat, "remove"), key);
        }

        /// <summary>
        /// Adds the specified key to the storage, or updates that key's value if it already exists asynchronously.
        /// </summary>
        /// <param name="key">The key of the item to set in localStorage.</param>
        /// <param name="value">The value to set in the localStorage.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        public async Task SetItemAsync(string key, string value)
        {
            await JS.InvokeVoidAsync(string.Format(LocalStorageIdFormat, "set"), key, value);
        }

        /// <summary>
        /// Serializes the specified <paramref name="value"/> and 
        /// invokes the localStorage.setItem function asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        /// <param name="key">The key of the item to set in localStorage.</param>
        /// <param name="value">The value to serialize and set in the localStorage.</param>
        /// <param name="options">Options to control serialization behaviour.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        public Task SetItemAsync<T>(string key, T value, JsonSerializerOptions? options = null)
        {
            var json = JsonSerializer.Serialize(value, options);
            return SetItemAsync(key, json);
        }

        /// <summary>
        /// Returns a previously stored authorization token asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        public Task<string> GetAuthorizationTokenAsync() => GetItemAsync(AuthorizationTokenKey);

        /// <summary>
        /// Returns a previously stored custom authorization token asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the authorization token to retrieve.</typeparam>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        public Task<T?> GetAuthorizationTokenAsync<T>() => GetItemAsync<T>(AuthorizationTokenKey);

        /// <summary>
        /// Removes a previously stored authorization token asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        public Task RemoveAuthorizationTokenAsync() => RemoveItemAsync(AuthorizationTokenKey);

        /// <summary>
        /// Adds the specified authorization token to the storage asynchronously.
        /// </summary>
        /// <param name="token">The authorization token to store.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        public Task SetAuthorizationTokenAsync(string token) => SetItemAsync(AuthorizationTokenKey, token);

        /// <summary>
        /// Adds the specified custom authorization token to the storage asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the authorization token to store.</typeparam>
        /// <param name="value">The value of the authorization token to store.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        public Task SetAuthorizationTokenAsync<T>(T value) => SetItemAsync<T>(AuthorizationTokenKey, value);

        #endregion
    }

}
