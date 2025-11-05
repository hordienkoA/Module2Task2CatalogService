using Catalog.BLL.Services;
using Catalog.Contracts.DTOs;
using Catalog.Service.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Catalog.Tests.Controllers
{
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly Mock<ILogger<CategoryController>> _mockLogger;
        private readonly CategoryController _controller;

        public CategoryControllerTests()
        {
            _mockCategoryService = new Mock<ICategoryService>();
            _mockLogger = new Mock<ILogger<CategoryController>>();
            _controller = new CategoryController(_mockCategoryService.Object, _mockLogger.Object);
        }


        [Fact]
        public async Task Get_ReturnsNotFound_WhenCategoryDoesNotExist()
        {
            var categoryId = 1;
            _mockCategoryService.Setup(s => s.GetAsync(categoryId)).ReturnsAsync((CategoryDto?)null);

            var result = await _controller.Get(categoryId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetList_ReturnsOk_WithCategories()
        {
            var categories = new List<CategoryDto>
            {
                new CategoryDto(1,"Category 1", "", null),
                new CategoryDto( 2, "Category 2", "", null )
            };
            _mockCategoryService.Setup(s => s.ListAsync()).ReturnsAsync(categories);

            var result = await _controller.GetList();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCategories = Assert.IsType<List<CategoryDto>>(okResult.Value);
            Assert.Equal(2, returnedCategories.Count);
        }

        [Fact]
        public async Task Add_ReturnsOk_WhenCategoryIsAdded()
        {
            var createCategoryDto = new CreateCategoryDto("New Category", "", null);
            var categoryDto = new CategoryDto(1,"New Category", "", null);
            _mockCategoryService.Setup(s => s.AddAsync(createCategoryDto)).ReturnsAsync(categoryDto);

            var result = await _controller.Add(createCategoryDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCategory = Assert.IsType<CategoryDto>(okResult.Value);
            Assert.Equal("New Category", returnedCategory.Name);
        }

        [Fact]
        public async Task Update_ReturnsOk_WhenCategoryIsUpdated()
        {
            var updateCategoryDto = new UpdateCategoryDto(1,"Updated Category", "", null );

            var result = await _controller.Update(updateCategoryDto);

            Assert.IsType<OkResult>(result);
            _mockCategoryService.Verify(s => s.UpdateAsync(updateCategoryDto), Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsOk_WhenCategoryIsDeleted()
        {
            var categoryId = 1;

            var result = await _controller.Delete(categoryId);

            Assert.IsType<OkResult>(result);
            _mockCategoryService.Verify(s => s.DeleteAsync(categoryId), Times.Once);
        }
    }
}