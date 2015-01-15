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

        public HttpServiceClient(IUrlProvider urlProvider)
        {
            _urlProvider = urlProvider;
        }

        public async Task<TResponse> GetAsync<TResponse>(string resource)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_urlProvider.BackplaneUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync(resource);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsAsync<TResponse>();

                return default(TResponse);
            }
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string resource, TRequest request)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_urlProvider.BackplaneUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PostAsJsonAsync(resource, request);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsAsync<TResponse>();

                return default(TResponse);
            }
        }
    }
}