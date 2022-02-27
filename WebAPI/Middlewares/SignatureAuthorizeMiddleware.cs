using Common.Lib.Dtos;
using Common.Lib.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebAPI.Middlewares
{
    public class SignatureAuthorizeMiddleware
    {
        private readonly RequestDelegate _next;

        public SignatureAuthorizeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IUrlSignatureService urlSignatureService)
        {
            // should store list keys in db or app settings: query by ClientId
            var serviceApi = new ApiServiceDto
            {
                Name = "ProductService",
                ApiClientKey = new ApiClientKeyDto
                {
                    ClientId = "1BC00C3632D667BE53ED5D21E96B750675B2B6E4",
                    ClientSecret = "EBB62BF08017EA25CA378BF0FDE6B2BF2E994E33"
                }
            };
            urlSignatureService.CreateApiServiceInstance(serviceApi.Name, serviceApi.ApiClientKey.ClientId, serviceApi.ApiClientKey.ClientSecret);
            if (urlSignatureService.ValidateUrlSignature(httpContext))
            {
                await _next(httpContext);
            }
        }
    }
}