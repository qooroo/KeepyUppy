using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace KeepyUppy.Interop
{
    public interface IHttpServiceClient
    {
        Task<TResponse> GetAsync<TResponse>(string resource);
        Task<TResponse> PostAsync<TRequest, TResponse>(string resource, TRequest request);
    }

    public class HttpServiceClient : IHttpServiceClient
    {
        private readonly IUrlProvider _urlProvider;
        private readonly Func<HttpClient> _httpClient; 

        public HttpServiceClient(IUrlProvider urlProvider)
        {
            _urlProvider = urlProvider;
            _httpClient = () =>
            {
                var client = new HttpClient {BaseAddress = new Uri(_urlProvider.BackplaneUrl)};
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                return client;
            };
        }

        public async Task<TResponse> GetAsync<TResponse>(string resource)
        {
            using (var client = _httpClient())
            {
                var response = await client.GetAsync(resource);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<TResponse>();
            }
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string resource, TRequest request)
        {
            using (var client = _httpClient())
            {
                var response = await client.PostAsJsonAsync(resource, request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<TResponse>();
            }
        }
    }
}