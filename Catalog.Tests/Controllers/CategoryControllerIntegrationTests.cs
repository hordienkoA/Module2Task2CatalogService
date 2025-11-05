using Catalog.BLL.Services;
using Catalog.Contracts.DTOs;
using Catalog.DAL;
using Catalog.DAL.Repositories;
using Catalog.DAL.UnitOfWork;
using Catalog.Service.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Catalog.Tests.Controllers
{
    public class CategoryControllerIntegrationTests
    {
        private CategoryController CreateController(out CatalogDbContext db)
        {
            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase("CatalogTestDb_" + Guid.NewGuid()) // isolate tests
                .Options;

            db = new CatalogDbContext(options);
            var categoryRepo = new CategoryRepository(db);
            var uow = new EfUnitOfWork(db, categoryRepo, null!);
            var categoryService = new CategoryService(uow);
            var controller = new CategoryController(categoryService, new NullLogger<CategoryController>());
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("localhost:5001");
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper
                .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns<UrlActionContext>(ctx =>
                    $"https://fakehost/api/{ctx.Controller}/{ctx.Action}");

            controller.Url = mockUrlHelper.Object;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext        
            };

            return controller;
        }

        [Fact]
        public async Task Add_Then_Get_Should_Return_Same_Category()
        {
            var controller = CreateController(out var db);
            var dto = new CreateCategoryDto("Electronics", "", null);

            var addResult = await controller.Add(dto) as OkObjectResult;
            Assert.NotNull(addResult);
            var created = Assert.IsType<CategoryDto>(addResult.Value);

            var getResult = await controller.Get(created.Id) as OkObjectResult;
            Assert.NotNull(getResult);
            Assert.True(getResult.Value is not null);
            Assert.True(getResult.StatusCode == 200);
        }
    }
  }