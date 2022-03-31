using BlazorFormManager.Components.Web;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorFormManager.Demo.Client.Services
{
    public class PaginationHttpClient
    {
        private string _baseUrl;
        private readonly JsonSerializerOptions JsonIgnoreCase = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public PaginationHttpClient(HttpClient client)
        {
            Http = client;
        }

        public HttpClient Http { get; }

        public void SetBaseUrl(string baseUrl)
        {
            if (baseUrl.EndsWith('/'))
                baseUrl = baseUrl.TrimEnd('/');
            _baseUrl = baseUrl;
        }

        public Task<PaginationHttpResult<T>> GetFromJsonAsync<T>(string url, int page, int pageSize, bool? sorting = null, string sortColumn = null)
        {
            return Http.GetFromJsonAsync<PaginationHttpResult<T>>(GetFullUrl(url, page, pageSize, sorting, sortColumn), JsonIgnoreCase);
        }

        public Task<PaginationHttpResult<T>> GetFromJsonAsync<T>(string url, int page, int pageSize, TableCell[] headers)
        {
            return Http.GetFromJsonAsync<PaginationHttpResult<T>>(GetFullUrl(url, page, pageSize, headers), JsonIgnoreCase);
        }

        private string GetFullUrl(string url, int page, int pageSize, bool? sorting = null, string sortColumn = null)
        {
            url = $"{_baseUrl}/{url}?page={page}&pageSize={pageSize}";
            if (sorting.HasValue)
                url += $"&sort={sorting.Value.ToString().ToLower()}&column={sortColumn}";
            return url;
        }

        private string GetFullUrl(string url, int page, int pageSize, TableCell[] headers)
        {
            var url1 = string.Join("&", headers.Select(hdr => $"columns[]={hdr.Name}"));
            var url2 = string.Join("&", headers.Select(hdr => $"sorts[]={(hdr.SortAscending == null ? "" : $"{hdr.SortAscending.Value.ToString().ToLower()}")}"));
            url = $"{_baseUrl}/{url}?page={page}&pageSize={pageSize}&{url1}&{url2}";
            return url;
        }
    }
}
