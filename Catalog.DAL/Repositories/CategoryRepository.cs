using Catalog.Contracts.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.DAL.Repositories
{
    public class CategoryRepository(CatalogDbContext db) : ICategoryRepository
    {
        private readonly CatalogDbContext db = db;

        public async Task AddAsync(Category entity)
        {
            await db.Categories.AddAsync(entity);
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludingId = null)
        {
            var q = db.Categories.AsQueryable();
            if (excludingId.HasValue)
            {
                q = q.Where(c => c.Id != excludingId.Value);
            }

            return await q.AnyAsync(c => c.Name == name);
        }

        public async Task<Category?> GetAsync(int id)
        {
            return await db.Categories.FindAsync(id);
        }

        public async Task<Category?> GetWithChildrenAsync(int id)
        {
            return await db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> HasChildrenAsync(int id)
        {
            var anySub = await db.Categories.AnyAsync(c => c.ParentCategoryId == id);
            var anyProducts = await db.Products.AnyAsync(p => p.CategoryId == id);
            return anySub || anyProducts;
        }

        public async Task<IReadOnlyList<Category>> ListAsync()
        {
            return await db.Categories.AsNoTracking().ToListAsync();
        }

        public void Remove(Category entity)
        {
            db.Categories.Remove(entity);
        }

        public void Update(Category entity)
        {
            db.Categories.Update(entity);
        }
    }
}
