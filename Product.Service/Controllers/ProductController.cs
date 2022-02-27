using Common.Lib;
using Common.Lib.Dtos;
using Common.Lib.Services;
using Microsoft.AspNetCore.Mvc;
using Product.Service.Dtos;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Product.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IHttpRequestFactory _httpRequestFactory;

        public ProductController(IHttpRequestFactory httpRequestFactory)
        {
            _httpRequestFactory = httpRequestFactory;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> Get()
        {
            // should store keys in db or app setting
            var serviceApi = new ApiServiceDto
            {
                Name = "ProductService",
                ApiClientKey = new ApiClientKeyDto
                {
                    ClientId = "1BC00C3632D667BE53ED5D21E96B750675B2B6E4",
                    ClientSecret = "EBB62BF08017EA25CA378BF0FDE6B2BF2E994E33"
                }
            };

            // call api to verify signature
            var endpoint = $"https://localhost:44330/api/{Constants.UrlApis.Product.GET_ALL}";
            var result = await _httpRequestFactory.SendWithSecurityAsync<DataResponse<IEnumerable<ProductDto>>>(serviceApi, endpoint, HttpMethod.Get);

            return Ok(result);
        }
    }
}