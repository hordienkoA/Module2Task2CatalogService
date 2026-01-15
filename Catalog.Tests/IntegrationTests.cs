using Catalog.DAL;
using Catalog.DAL.Repositories;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Tests
{
    public class IntegrationTests : IAsyncLifetime
    {
        private CatalogDbContext db;

        public async Task InitializeAsync()
        {
            var opts = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase("CatalogTestDb")
                .Options;
            db = new CatalogDbContext(opts);

            db.Categories.Add(new Category { Name = "Test", Image = "test.com" });
            await db.SaveChangesAsync();
        }
        public Task DisposeAsync()
        {
            db.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task CategoryRepositoryListReturnsList()
        {
            var repo = new CategoryRepository(db);
            var list = await repo.ListAsync();
            Assert.Single(list);
            Assert.Equal("Test", list[0].Name);
        }


    }
}
