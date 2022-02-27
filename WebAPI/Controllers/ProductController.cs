using Common.Lib.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Dtos;

namespace WebAPI.Controllers
{
    [Route("api/product")]
    public class ProductController : ControllerBase
    {
        private static readonly ProductDto[] Products = new[]
        {
            new ProductDto
            {
                Id = 1,
                Name = "Iphone 13 ProMax",
                Supplier = "Iphone"
            },
            new ProductDto
            {
                Id = 2,
                Name = "Galaxy A12",
                Supplier = "Samsung"
            }
        };

        private readonly ILogger<ProductController> _logger;

        public ProductController(ILogger<ProductController> logger)
        {
            _logger = logger;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllProductAsync()
        {
            var dataResponse = new DataResponse<IEnumerable<ProductDto>>(200, Products);

            return Ok(await Task.FromResult(dataResponse));
        }
    }
}