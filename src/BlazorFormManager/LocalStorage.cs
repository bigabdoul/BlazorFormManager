// Copyright (c) Karfamsoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.JSInterop;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorFormManager
{
    /// <summary>
    /// Represents an instance of a limited JavaScript runtime to which 
    /// localStorage-related calls may be dispatched through the BlazorFormManager.js script.
    /// </summary>
    public sealed class LocalStorage
    {
        #region fields

        private readonly IJSRuntime JS; 
        private const string LOCAL_STORAGE_ID_FORMAT = "BlazorFormManager.localStorage{0}Item";
        private const string LOCAL_STORAGE_TOKEN_KEY = "BlazorFormManager.AuthorizationToken";

        #endregion

        #region constructor

        /// <summary>
        /// Create a new instance of the <see cref="LocalStorage"/> class using the 
        /// specified <see cref="IJSRuntime"/>.
        /// </summary>
        /// <param name="jSRuntime">The JavaScript runtime instance to which calls are dispatched.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="jSRuntime"/> is null.</exception>
        public static LocalStorage Create(IJSRuntime jSRuntime) => new LocalStorage(jSRuntime);

        /// <summary>
        /// Initializes new instance of the <see cref="LocalStorage"/> class using 
        /// the specified <see cref="IJSRuntime"/>.
        /// </summary>
        /// <param name="jSRuntime">The JavaScript runtime instance to which calls are dispatched.</param>
        /// <exception cref="ArgumentNullException"><paramref name="jSRuntime"/> is null.</exception>
        private LocalStorage(IJSRuntime jSRuntime)
        {
            JS = jSRuntime ?? throw new ArgumentNullException(nameof(jSRuntime));
        }

        #endregion

        #region methods

        /// <summary>
        /// Serializes the specified <paramref name="value"/> and 
        /// invokes the localStorage.setItem function asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        /// <param name="key">The key of the item to set in localStorage.</param>
        /// <param name="value">The value to serialize and set in the localStorage.</param>
        /// <param name="options">Options to control serialization behaviour.</param>
        /// <returns></returns>
        public Task SetItemAsync<T>(string key, T value, JsonSerializerOptions options = null)
        {
            var json = JsonSerializer.Serialize(value, options);
            return SetItemAsync(key, json);
        }

        /// <summary>
        /// Adds the specified key to the storage, or updates that key's value if it already exists asynchronously.
        /// </summary>
        /// <param name="key">The key of the item to set in localStorage.</param>
        /// <param name="value">The value to set in the localStorage.</param>
        /// <returns></returns>
        public async Task SetItemAsync(string key, string value)
        {
            await JS.InvokeVoidAsync(string.Format(LOCAL_STORAGE_ID_FORMAT, "Set"), key, value);
        }

        /// <summary>
        /// Returns the specified key's value asynchronously.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns></returns>
        public async Task<string> GetItemAsync(string key)
        {
            return await JS.InvokeAsync<string>(string.Format(LOCAL_STORAGE_ID_FORMAT, "Get"), key);
        }

        /// <summary>
        /// Retrieves the specified key's value, deserializes and returns it asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the stored value.</typeparam>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="options">Options to control the behaviour during deserialization.</param>
        /// <returns></returns>
        public async Task<T> GetItemAsync<T>(string key, JsonSerializerOptions options = null)
        {
            var json = await GetItemAsync(key);
            if (json == null) return default;
            return JsonSerializer.Deserialize<T>(json, options);
        }

        /// <summary>
        /// Removes the key from the storage asynchronously.
        /// </summary>
        /// <param name="key">The key of the value to remove.</param>
        /// <returns></returns>
        public async Task RemoveItemAsync(string key)
        {
            await JS.InvokeVoidAsync(string.Format(LOCAL_STORAGE_ID_FORMAT, "Remove"), key);
        }

        /// <summary>
        /// Adds the specified authorization token to the storage asynchronously.
        /// </summary>
        /// <param name="token">The authorization token to store.</param>
        /// <returns></returns>
        public Task SetAuthorizationTokenAsync(string token) => SetItemAsync(LOCAL_STORAGE_TOKEN_KEY, token);

        /// <summary>
        /// Returns a previously stored authorization token asynchronously.
        /// </summary>
        /// <returns></returns>
        public Task<string> GetAuthorizationTokenAsync() => GetItemAsync(LOCAL_STORAGE_TOKEN_KEY);

        /// <summary>
        /// Removes a previously stored authorization token asynchronously.
        /// </summary>
        /// <returns></returns>
        public Task RemoveAuthorizationTokenAsync() => RemoveItemAsync(LOCAL_STORAGE_TOKEN_KEY);

        #endregion
    }
}
