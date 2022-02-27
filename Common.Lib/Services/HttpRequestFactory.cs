using Common.Lib.Dtos;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Common.Lib.Services
{
    public class HttpRequestFactory : IHttpRequestFactory
    {
        private readonly HttpClient _httpClient;
        private readonly IUrlSignatureService _urlSignatureService;

        public HttpRequestFactory(HttpClient httpClient = null, IUrlSignatureService urlSignatureService = null)
        {
            _httpClient = httpClient;
            _urlSignatureService = urlSignatureService;
        }

        private HttpRequestMessage CreateRequestWithUrlSignature(ApiServiceDto apiService, string endPoint, HttpMethod httpMethod)
        {
            _urlSignatureService.CreateApiServiceInstance(apiService.Name, apiService.ApiClientKey.ClientId, apiService.ApiClientKey.ClientSecret);
            endPoint = _urlSignatureService.CreateUrlSignature(endPoint);
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(endPoint)
            };
            request.Method = httpMethod;

            return request;
        }

        public async Task<T> SendWithSecurityAsync<T>(ApiServiceDto apiService, string endPoint, HttpMethod httpMethod, object payload = null)
        {
            var request = CreateRequestWithUrlSignature(apiService, endPoint, httpMethod);
            request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseStream = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var data = JsonConvert.DeserializeObject<T>(responseStream);

            return data;
        }
    }
}