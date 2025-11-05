using Catalog.Domain.Entities;

namespace Catalog.Contracts.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IReadOnlyList<Product?>> ListByCategoryAsync(int categoryId);
        Task<IReadOnlyList<Product?>> ListAsync(int? categoryId, int page, int pageSize);
    }
}
