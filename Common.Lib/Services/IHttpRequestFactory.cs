using Common.Lib.Dtos;
using System.Net.Http;
using System.Threading.Tasks;

namespace Common.Lib.Services
{
    public interface IHttpRequestFactory
    {
        Task<T> SendWithSecurityAsync<T>(ApiServiceDto apiService, string endPoint, HttpMethod httpMethod, object payload = null);
    }
}