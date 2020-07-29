using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorFormManager.Demo.Client.Services
{
    public interface IJwtAccessTokenProvider
    {
        Task<string> GetTokenAsync();
    }

    public class JwtAccessTokenProvider : IJwtAccessTokenProvider
    {
        private readonly HttpClient _http;

        public JwtAccessTokenProvider(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> GetTokenAsync()
        {
            var response = await _http.GetAsync("api/token");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
