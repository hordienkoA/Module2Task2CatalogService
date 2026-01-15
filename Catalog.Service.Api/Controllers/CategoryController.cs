using Catalog.BLL.Services;
using Catalog.Contracts.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Service.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController(ICategoryService categoryService) : ControllerBase
    {
        private readonly ICategoryService categoryService = categoryService;

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            var item = await categoryService.GetAsync(id);
            if (item == null)
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
            var items = await categoryService.ListAsync();
            return Ok(items);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Add(CreateCategoryDto category)
        {
            try
            {
                var result = await categoryService.AddAsync(category);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Diagnostic: show incoming Authorization header and authenticate explicitly with Bearer scheme
        [HttpGet("auth-check")]
        public async Task<IActionResult> AuthCheck()
        {
            var incomingAuth = Request.Headers.TryGetValue("Authorization", out var h) ? h.ToString() : null;

            // Force Authenticate with JwtBearer scheme (avoids NoResult when default scheme not set)
            var authResult = await HttpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            var principal = authResult.Principal;

            return Ok(new
            {
                IncomingAuthorizationHeader = incomingAuth,
                Authenticated = principal?.Identity?.IsAuthenticated == true,
                Scheme = authResult.Ticket?.AuthenticationScheme,
                Failure = authResult.Failure?.Message,
                Claims = principal?.Claims.Select(c => new { c.Type, c.Value })
            });
        }

        [HttpPut]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Update(UpdateCategoryDto category)
        {
            try
            {
                await categoryService.UpdateAsync(category);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                await categoryService.DeleteAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
