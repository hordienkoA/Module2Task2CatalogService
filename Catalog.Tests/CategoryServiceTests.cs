using Catalog.BLL.Services;
using Catalog.Contracts.DTOs;
using Catalog.Contracts.Interfaces;
using Catalog.DAL.UnitOfWork;
using Catalog.Domain.Entities;
using Moq;

namespace Catalog.Tests
{
    public class CategoryServiceTests
    {
        [Fact]
        public async Task AddAsync_Valid_CreatesCategory()
        {
            var uowMock = new Mock<IUnitOfWork>();
            var catRepoMock = new Mock<ICategoryRepository>();
            catRepoMock.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
            uowMock.SetupGet(u => u.CategoryRepository).Returns(catRepoMock.Object);
            uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var svc = new CategoryService(uowMock.Object);
            var dto = new CreateCategoryDto("New cat", null, null);
            var result = await svc.AddAsync(dto);

            Assert.Equal("New cat", result.Name);
            catRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
            uowMock.Verify(u=>u.SaveChangesAsync(), Times.Once);
        }
    }
}
