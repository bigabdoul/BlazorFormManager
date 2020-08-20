using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorFormManager.Demo.Client.Services
{
    public interface IRequestHeadersProvider
    {
        Task<IDictionary<string, object>> CreateAsync();
    }

    public class RequestHeadersProvider : IRequestHeadersProvider
    {
        private readonly IAccessTokenProvider _provider;

        public RequestHeadersProvider(IAccessTokenProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Returns a dictionary containing the 'authorization' request header and an anti-forgery request token.
        /// </summary>
        /// <returns></returns>
        public async Task<IDictionary<string, object>> CreateAsync()
        {
            var tokenResponse = await _provider.RequestAccessToken();

            if (tokenResponse.TryGetToken(out var token))
            {
                var headers = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "authorization", $"Bearer {token.Value}" },
                    { "x-requested-with", "XMLHttpRequest" },
                    { "x-powered-by", "BlazorFormManager" },
                };

                return headers;
            }
            else
            {
                throw new InvalidOperationException("Could not get access token.");
            }
        }
    }
}
