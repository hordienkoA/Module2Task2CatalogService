using Catalog.Contracts.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.DAL.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CatalogDbContext _db;
        public CategoryRepository(CatalogDbContext db) => _db = db;



        public async Task AddAsync(Category entity) => await _db.Categories.AddAsync(entity);
        public async Task<bool> ExistsByNameAsync(string name, int? excludingId = null)
        {
            var q = _db.Categories.AsQueryable();
            if (excludingId.HasValue)
                q = q.Where(c => c.Id != excludingId.Value);
            return await q.AnyAsync(c => c.Name == name);
        }

        public async Task<Category> GetAsync(int id) => await _db.Categories.FindAsync(id);

        public async Task<Category> GetWithChildrenAsync(int id) => await _db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);

        public async Task<bool> HasChildrenAsync(int id)
        {
            var anySub = await _db.Categories.AnyAsync(c => c.ParentCategoryId == id);
            var anyProducts = await _db.Products.AnyAsync(p => p.CategoryId == id);
            return anySub || anyProducts;
        }

        public async Task<IReadOnlyList<Category>> ListAsync() => await _db.Categories.AsNoTracking().ToListAsync();

        public void Remove(Category entity) => _db.Categories.Remove(entity);

        public void Update(Category entity) => _db.Categories.Update(entity);
    }
}
