using Catalog.Contracts.Interfaces;

namespace Catalog.DAL.UnitOfWork
{
    public interface IUnitOfWork
    {
        ICategoryRepository CategoryRepository { get; }
        IProductRepository ProductRepository { get; }
        Task<int> SaveChangesAsync();
    }
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly CatalogDbContext _db;
        public ICategoryRepository CategoryRepository { get; }
        public IProductRepository ProductRepository { get; }

        public EfUnitOfWork(CatalogDbContext db, ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            _db = db;
            CategoryRepository = categoryRepository;
            ProductRepository = productRepository;
        }
        public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
    }
}
