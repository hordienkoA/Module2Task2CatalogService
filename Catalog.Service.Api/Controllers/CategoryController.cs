using Catalog.BLL.Services;
using Catalog.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Service.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController: ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;
        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            var item = await _categoryService.GetAsync(id);
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
                    products = $"{baseUrl}{Url.Action("GetList", "Product", new { categoryId = id })}"
                }
            };
            return Ok(response);
        }

        [HttpGet("List")]
        public async Task<IActionResult> GetList()
        {
            var items = await _categoryService.ListAsync();
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Add(CreateCategoryDto category)
        {
            try
            {
                var result = await _categoryService.AddAsync(category);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateCategoryDto category)
        {
            try
            {
                await _categoryService.UpdateAsync(category);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                await _categoryService.DeleteAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
