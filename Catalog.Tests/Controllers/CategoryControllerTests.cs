using Catalog.BLL.Services;
using Catalog.Contracts.DTOs;
using Catalog.Service.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Catalog.Tests.Controllers
{
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryService> mockCategoryService;
        private readonly CategoryController controller;

        public CategoryControllerTests()
        {
            mockCategoryService = new Mock<ICategoryService>();
            controller = new CategoryController(mockCategoryService.Object);
        }


        [Fact]
        public async Task GetReturnsNotFoundWhenCategoryDoesNotExist()
        {
            var categoryId = 1;
            mockCategoryService.Setup(s => s.GetAsync(categoryId)).ReturnsAsync((CategoryDto?)null);

            var result = await controller.Get(categoryId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetListReturnsOkWithCategories()
        {
            var categories = new List<CategoryDto>
            {
                new (1,"Category 1", "", null),
                new ( 2, "Category 2", "", null )
            };
            mockCategoryService.Setup(s => s.ListAsync()).ReturnsAsync(categories);

            var result = await controller.GetList();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCategories = Assert.IsType<List<CategoryDto>>(okResult.Value);
            Assert.Equal(2, returnedCategories.Count);
        }

        [Fact]
        public async Task AddReturnsOkWhenCategoryIsAdded()
        {
            var createCategoryDto = new CreateCategoryDto("New Category", "", null);
            var categoryDto = new CategoryDto(1, "New Category", "", null);
            mockCategoryService.Setup(s => s.AddAsync(createCategoryDto)).ReturnsAsync(categoryDto);

            var result = await controller.Add(createCategoryDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCategory = Assert.IsType<CategoryDto>(okResult.Value);
            Assert.Equal("New Category", returnedCategory.Name);
        }

        [Fact]
        public async Task UpdateReturnsOkWhenCategoryIsUpdated()
        {
            var updateCategoryDto = new UpdateCategoryDto(1, "Updated Category", "", null);

            var result = await controller.Update(updateCategoryDto);

            Assert.IsType<OkResult>(result);
            mockCategoryService.Verify(s => s.UpdateAsync(updateCategoryDto), Times.Once);
        }

        [Fact]
        public async Task DeleteReturnsOkWhenCategoryIsDeleted()
        {
            var categoryId = 1;

            var result = await controller.Delete(categoryId);

            Assert.IsType<OkResult>(result);
            mockCategoryService.Verify(s => s.DeleteAsync(categoryId), Times.Once);
        }
    }
}
