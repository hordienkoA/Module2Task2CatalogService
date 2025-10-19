using Catalog.Domain.Entities;

namespace Catalog.Contracts.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IReadOnlyList<Product>> ListByCategoryAsync(int categoryId);
    }
}
