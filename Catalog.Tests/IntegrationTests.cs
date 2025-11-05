using Catalog.DAL;
using Catalog.DAL.Repositories;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Tests
{
    public class IntegrationTests : IAsyncLifetime
    {
        private CatalogDbContext _db;

        public async Task InitializeAsync()
        {
            var opts = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase("CatalogTestDb")
                .Options;
            _db = new CatalogDbContext(opts);

            _db.Categories.Add(new Category { Name = "Test", Image= "test.com" });
            await _db.SaveChangesAsync();
        }
        public Task DisposeAsync()
        {
            _db.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task CategoryRepository_List_ReturnsList()
        {
            var repo = new CategoryRepository(_db);
            var list = await repo.ListAsync();
            Assert.Single(list);
            Assert.Equal("Test", list.First().Name);
        }


    }
}
