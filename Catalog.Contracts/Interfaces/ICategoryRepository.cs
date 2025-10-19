using Catalog.Domain.Entities;

namespace Catalog.Contracts.Interfaces
{
    public interface ICategoryRepository: IRepository<Category>
    {
        Task<Category> GetWithChildrenAsync(int id);
        Task<bool> HasChildrenAsync(int id);
        Task<bool> ExistsByNameAsync(string name, int? excludingId = null);
    }
}
