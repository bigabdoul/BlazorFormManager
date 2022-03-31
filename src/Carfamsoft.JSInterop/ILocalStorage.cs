using System.Text.Json;
using System.Threading.Tasks;

namespace Carfamsoft.JSInterop
{
    /// <summary>
    /// Represents an instance of a JavaScript localStorage to which calls may be dispatched.
    /// </summary>
    public interface ILocalStorage
    {
        #region properties

        /// <summary>
        /// Gets or sets the application name.
        /// </summary>
        string? ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the authorization token key name.
        /// </summary>
        string AuthorizationTokenKey { get; set; }

        /// <summary>
        /// Gets or sets the globalThis module (can be the window object or other supported environments).
        /// </summary>
        string GlobalThis { get; set; }

        #endregion

        /// <summary>
        /// Returns the specified key's value asynchronously.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        Task<string> GetItemAsync(string key);

        /// <summary>
        /// Retrieves the specified key's value, deserializes and returns it asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the stored value.</typeparam>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="options">Options to control the behaviour during deserialization.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        Task<T?> GetItemAsync<T>(string key, JsonSerializerOptions? options = null);

        /// <summary>
        /// Removes the key from the storage asynchronously.
        /// </summary>
        /// <param name="key">The key of the value to remove.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        Task RemoveItemAsync(string key);

        /// <summary>
        /// Adds the specified key to the storage, or updates that key's value if it already exists asynchronously.
        /// </summary>
        /// <param name="key">The key of the item to set in localStorage.</param>
        /// <param name="value">The value to set in the localStorage.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        Task SetItemAsync(string key, string value);

        /// <summary>
        /// Serializes the specified <paramref name="value"/> and 
        /// invokes the localStorage.setItem function asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        /// <param name="key">The key of the item to set in localStorage.</param>
        /// <param name="value">The value to serialize and set in the localStorage.</param>
        /// <param name="options">Options to control serialization behaviour.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        Task SetItemAsync<T>(string key, T value, JsonSerializerOptions? options = null);

        /// <summary>
        /// Returns a previously stored authorization token asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        Task<string> GetAuthorizationTokenAsync();

        /// <summary>
        /// Returns a previously stored custom authorization token asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the authorization token to retrieve.</typeparam>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        Task<T?> GetAuthorizationTokenAsync<T>();

        /// <summary>
        /// Removes a previously stored authorization token asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        Task RemoveAuthorizationTokenAsync();

        /// <summary>
        /// Adds the specified authorization token to the storage asynchronously.
        /// </summary>
        /// <param name="token">The authorization token to store.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        Task SetAuthorizationTokenAsync(string token);

        /// <summary>
        /// Adds the specified custom authorization token to the storage asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the authorization token to store.</typeparam>
        /// <param name="value">The value of the authorization token to store.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous invocation operation.</returns>
        Task SetAuthorizationTokenAsync<T>(T value);
    }
}