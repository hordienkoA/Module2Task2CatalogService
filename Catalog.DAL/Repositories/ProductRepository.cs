using Catalog.Contracts.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.DAL.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogDbContext _db;
        public ProductRepository(CatalogDbContext db) => _db = db;

        public async Task AddAsync(Product entity) => await _db.Products.AddAsync(entity);

        public async Task<Product?> GetAsync(int id) => await _db.Products.FindAsync(id);


        public async Task<IReadOnlyList<Product>> ListAsync() => await _db.Products.AsNoTracking().ToListAsync();

        public async Task<IReadOnlyList<Product?>> ListAsync(int? categoryId, int page=1, int pageSize= 20)=>
                    categoryId!=null?
                   await _db.Products.Where(p => p.CategoryId == categoryId).Skip((page-1)*pageSize).Take(pageSize).AsNoTracking().ToListAsync():
                   await _db.Products.Skip((page-1)*pageSize).Take(pageSize).AsNoTracking().ToListAsync();
        

        public async Task<IReadOnlyList<Product?>> ListByCategoryAsync(int categoryId) =>
            await _db.Products.Where(p => p.CategoryId == categoryId).AsNoTracking().ToListAsync();

        public void Remove(Product entity) => _db.Products.Remove(entity);

        public void Update(Product entity) => _db.Products.Update(entity);
    }
}
