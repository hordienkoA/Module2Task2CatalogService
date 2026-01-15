using Catalog.BLL.Services;
using Catalog.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Service.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController(IProductService productService) : ControllerBase
    {
        private readonly IProductService productService = productService;

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            var item = await productService.GetAsync(id);
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

            var items = await productService.ListAsync(categoryId, page, pageSize);
            return Ok(items);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Add(CreateProductDto product)
        {
            try
            {
                var result = await productService.AddAsync(product);
                return Ok(result);
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Update(UpdateProductDto product)
        {
            try
            {
                await productService.UpdateAsync(product);
                return Ok();
            }
            catch(Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            try
            {
                await productService.DeleteAsync(id);
                return Ok();
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }
    }
}
