using Catalog.Contracts.DTOs;
using Catalog.DAL.UnitOfWork;
using Catalog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.BLL.Services
{
    public interface ICategoryService
    {
        Task<CategoryDto> GetAsync(int id);
        Task<IReadOnlyList<CategoryDto>> ListAsync();
        Task<CategoryDto> AddAsync(CreateCategoryDto dto);
        Task UpdateAsync(UpdateCategoryDto dto);
        Task DeleteAsync(int id);
    }
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _uow;
        public CategoryService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<CategoryDto> AddAsync(CreateCategoryDto dto)
        {
            // simple validation. Can be rewriten to Fluent validation in future.
            if (string.IsNullOrEmpty(dto.Name) || dto.Name.Length > 50)
                throw new ArgumentException("Invalid category name");
            if (await _uow.CategoryRepository.ExistsByNameAsync(dto.Name))
                throw new InvalidOperationException("Category with same name already exists");
            var entity = new Category { Name = dto.Name.Trim(), Image = dto.Image, ParentCategoryId = dto.ParentCategoryId };
            await _uow.CategoryRepository.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return new CategoryDto(entity.Id, entity.Name, entity.Image, entity.ParentCategoryId);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _uow.CategoryRepository.GetAsync(id);
            if (entity == null) throw new ArgumentException("Category not found");
            if (await _uow.CategoryRepository.HasChildrenAsync(id))
                throw new InvalidOperationException("Category has children or products");
            _uow.CategoryRepository.Remove(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task<CategoryDto> GetAsync(int id)
        {
            var e = await _uow.CategoryRepository.GetAsync(id);
            if (e == null) return null;
            return new CategoryDto(e.Id, e.Name, e.Image, e.ParentCategoryId);
        }

        public async Task<IReadOnlyList<CategoryDto>> ListAsync()
        {
            var list = await _uow.CategoryRepository.ListAsync();
            return list.Select(c => new CategoryDto(c.Id, c.Name, c.Image, c.ParentCategoryId)).ToList();
        }

        public async Task UpdateAsync(UpdateCategoryDto dto)
        {
            var entity = await _uow.CategoryRepository.GetAsync(dto.Id);
            if (entity == null) throw new ArgumentException("Category not found");


            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 50)
                throw new ArgumentException("Invalid category name");


            if (await _uow.CategoryRepository.ExistsByNameAsync(dto.Name, dto.Id))
                throw new InvalidOperationException("Category with same name already exists");

            entity.Name = dto.Name.Trim();
            entity.Image = dto.Image;
            entity.ParentCategoryId = dto.ParentCategoryId;

            _uow.CategoryRepository.Update(entity);
            await _uow.SaveChangesAsync();
        }
    }
}
