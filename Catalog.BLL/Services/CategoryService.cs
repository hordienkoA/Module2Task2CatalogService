using Catalog.Contracts.DTOs;
using Catalog.DAL.UnitOfWork;
using Catalog.Domain.Entities;

namespace Catalog.BLL.Services
{
    public interface ICategoryService
    {
        Task<CategoryDto?> GetAsync(int id);
        Task<IReadOnlyList<CategoryDto>> ListAsync();
        Task<CategoryDto> AddAsync(CreateCategoryDto dto);
        Task UpdateAsync(UpdateCategoryDto dto);
        Task DeleteAsync(int id);
    }
    public class CategoryService(IUnitOfWork uow) : ICategoryService
    {
        private readonly IUnitOfWork uow = uow;

        public async Task<CategoryDto> AddAsync(CreateCategoryDto dto)
        {
            // simple validation. Can be rewriten to Fluent validation in future.
            if (string.IsNullOrEmpty(dto.Name) || dto.Name.Length > 50)
            {
                throw new ArgumentException("Invalid category name");
            }

            if (await uow.CategoryRepository.ExistsByNameAsync(dto.Name))
            {
                throw new InvalidOperationException("Category with same name already exists");
            }

            var entity = new Category { Name = dto.Name.Trim(), Image = dto.Image, ParentCategoryId = dto.ParentCategoryId };
            await uow.CategoryRepository.AddAsync(entity);
            await uow.SaveChangesAsync();
            return new CategoryDto(entity.Id, entity.Name, entity.Image, entity.ParentCategoryId);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await uow.CategoryRepository.GetAsync(id);
            if (entity == null)
            {
                throw new ArgumentException("Category not found");
            }

            uow.CategoryRepository.Remove(entity);
            await uow.SaveChangesAsync();
        }

        public async Task<CategoryDto?> GetAsync(int id)
        {
            var e = await uow.CategoryRepository.GetAsync(id);
            if (e == null)
            {
                return null;
            }

            return new CategoryDto(e.Id, e.Name, e.Image, e.ParentCategoryId);
        }

        public async Task<IReadOnlyList<CategoryDto>> ListAsync()
        {
            var list = await uow.CategoryRepository.ListAsync();
            return [.. list.Select(c => new CategoryDto(c.Id, c.Name, c.Image, c.ParentCategoryId))];
        }

        public async Task UpdateAsync(UpdateCategoryDto dto)
        {
            var entity = await uow.CategoryRepository.GetAsync(dto.Id);
            if (entity == null)
            {
                throw new ArgumentException("Category not found");
            }

            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 50)
            {
                throw new ArgumentException("Invalid category name");
            }

            if (await uow.CategoryRepository.ExistsByNameAsync(dto.Name, dto.Id))
            {
                throw new InvalidOperationException("Category with same name already exists");
            }

            entity.Name = dto.Name.Trim();
            entity.Image = dto.Image;
            entity.ParentCategoryId = dto.ParentCategoryId;

            uow.CategoryRepository.Update(entity);
            await uow.SaveChangesAsync();
        }
    }
}
