using Catalog.Contracts.DTOs;
using Catalog.DAL.UnitOfWork;
using Catalog.Domain.Entities;

namespace Catalog.BLL.Services
{
    public interface IProductService
    {
        Task<ProductDto?> GetAsync(int id);
        Task<IReadOnlyList<ProductDto>> ListAsync();
        Task<ProductDto> AddAsync(CreateProductDto dto);
        Task UpdateAsync(UpdateProductDto dto);
        Task DeleteAsync(int id);
    }
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _uow;
        public ProductService(IUnitOfWork uow) => _uow = uow;

        public async Task<ProductDto> AddAsync(CreateProductDto dto)
        {
            if (string.IsNullOrEmpty(dto.Name) || dto.Name.Length > 50)
                throw new ArgumentException("Invalid product name");
            if (dto.Price < 0) throw new ArgumentException("Price must be >= 0");
            if (dto.Amount < 0) throw new ArgumentException("Amount must be >= 0");

            var cat = await _uow.CategoryRepository.GetAsync(dto.CategoryId);
            if (cat == null) throw new ArgumentException("Category not found");

            var entity = new Product
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                Image = dto.Image,
                CategoryId = dto.CategoryId,
                Price = dto.Price,
                Amount = dto.Amount
            };

            await _uow.ProductRepository.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return new ProductDto(entity.Id, entity.Name, entity.Description, entity.Image, entity.CategoryId, entity.Price, entity.Amount);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _uow.ProductRepository.GetAsync(id);
            if (entity == null) throw new ArgumentException("Product not found");

            _uow.ProductRepository.Remove(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task<ProductDto?> GetAsync(int id)
        {
            var e = await _uow.ProductRepository.GetAsync(id);
            if (e == null) return null;
            return new ProductDto(e.Id, e.Name, e.Description, e.Image, e.CategoryId, e.Price, e.Amount);
        }

        public async Task<IReadOnlyList<ProductDto>> ListAsync()
        {
            var list = await _uow.ProductRepository.ListAsync();
            return list.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Image, p.CategoryId, p.Price, p.Amount)).ToList();
        }

        public async Task UpdateAsync(UpdateProductDto dto)
        {
            var entity = await _uow.ProductRepository.GetAsync(dto.Id);
            if (entity == null) throw new ArgumentException("Product not found");

            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 50)
                throw new ArgumentException("Invalid product name");
            if (dto.Price < 0) throw new ArgumentException("Price must be >=0");
            if (dto.Amount < 0) throw new ArgumentException("Amount must be >=0");

            var cat = await _uow.CategoryRepository.GetAsync(dto.CategoryId);
            if (cat == null) throw new ArgumentException("Category not found");

            entity.Name = dto.Name.Trim();
            entity.Description = dto.Description;
            entity.Image = dto.Image;
            entity.CategoryId = dto.CategoryId;
            entity.Price = dto.Price;
            entity.Amount = dto.Amount;

            _uow.ProductRepository.Update(entity);
            await _uow.SaveChangesAsync();
        }
    }
}
