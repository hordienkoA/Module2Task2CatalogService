using Catalog.BLL.Services;
using Catalog.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Service.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;
        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            var item = await _productService.GetAsync(id);
            if(item == null)
            {
                return NotFound();
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var response = new
            {
                item.Id,
                item.Name,
                links = new
                {
                    self = $"{baseUrl}{Url.Action(nameof(Get), new { id })}",
                }
            };

            return Ok(response);
        }

        [HttpGet("List")]
        public async Task<IActionResult> GetList([FromQuery] int? categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Page and pageSize must be positive.");

            var items = await _productService.ListAsync(categoryId, page, pageSize);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Add(CreateProductDto product)
        {
            try
            {
                var result = await _productService.AddAsync(product);
                return Ok(result);
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateProductDto product)
        {
            try
            {
                await _productService.UpdateAsync(product);
                return Ok();
            }
            catch(Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            try
            {
                await _productService.DeleteAsync(id);
                return Ok();
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }
    }
}
