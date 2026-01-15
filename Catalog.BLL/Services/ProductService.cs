using CartService.API.MessageBroker.Messages;
using CartService.BLL.Interfaces;
using Catalog.Contracts.DTOs;
using Catalog.DAL.UnitOfWork;
using Catalog.Domain.Entities;

namespace Catalog.BLL.Services
{
    public interface IProductService
    {
        Task<ProductDto?> GetAsync(int id);
        Task<IReadOnlyList<ProductDto>> ListAsync(int? categoryId, int page, int pageSize);
        Task<ProductDto> AddAsync(CreateProductDto dto);
        Task UpdateAsync(UpdateProductDto dto);
        Task DeleteAsync(int id);
    }
    public class ProductService(IUnitOfWork uow, IRabbitMqPublisher rabbitMqPublisher) : IProductService
    {
        private readonly IUnitOfWork uow = uow;
        private readonly IRabbitMqPublisher rabbitMqPublisher = rabbitMqPublisher;

        public async Task<ProductDto> AddAsync(CreateProductDto dto)
        {
            if (string.IsNullOrEmpty(dto.Name) || dto.Name.Length > 50)
            {
                throw new ArgumentException("Invalid product name");
            }

            if (dto.Price < 0)
            {
                throw new ArgumentException("Price must be >= 0");
            }

            if (dto.Amount < 0)
            {
                throw new ArgumentException("Amount must be >= 0");
            }

            var cat = await uow.CategoryRepository.GetAsync(dto.CategoryId);
            if (cat == null)
            {
                throw new ArgumentException("Category not found");
            }

            var entity = new Product
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                Image = dto.Image,
                CategoryId = dto.CategoryId,
                Price = dto.Price,
                Amount = dto.Amount
            };

            await uow.ProductRepository.AddAsync(entity);
            await uow.SaveChangesAsync();
            return new ProductDto(entity.Id, entity.Name, entity.Description, entity.Image, entity.CategoryId, entity.Price, entity.Amount);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await uow.ProductRepository.GetAsync(id);
            if (entity == null)
            {
                throw new ArgumentException("Product not found");
            }

            uow.ProductRepository.Remove(entity);
            await uow.SaveChangesAsync();
        }

        public async Task<ProductDto?> GetAsync(int id)
        {
            var e = await uow.ProductRepository.GetAsync(id);
            if (e == null)
            {
                return null;
            }

            return new ProductDto(e.Id, e.Name, e.Description, e.Image, e.CategoryId, e.Price, e.Amount);
        }

        public async Task<IReadOnlyList<ProductDto>> ListAsync(int? categoryId, int page, int pageSize)
        {
            var list = await uow.ProductRepository.ListAsync(categoryId, page, pageSize);
            return [.. list.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Image, p.CategoryId, p.Price, p.Amount))];
        }

        public async Task UpdateAsync(UpdateProductDto dto)
        {
            var entity = await uow.ProductRepository.GetAsync(dto.Id) ?? throw new ArgumentException("Product not found");
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 50)
            {
                throw new ArgumentException("Invalid product name");
            }

            if (dto.Price < 0)
            {
                throw new ArgumentException("Price must be >=0");
            }

            if (dto.Amount < 0)
            {
                throw new ArgumentException("Amount must be >=0");
            }

            if (await uow.CategoryRepository.GetAsync(dto.CategoryId) == null)
            {
                throw new ArgumentException("Category not found");
            }

            entity.Name = dto.Name.Trim();
            entity.Description = dto.Description;
            entity.Image = dto.Image;
            entity.CategoryId = dto.CategoryId;
            entity.Price = dto.Price;
            entity.Amount = dto.Amount;
            var message = new CatalogItemUpdatedEvent
            {
                ProductId = entity.Id,
                Name = entity.Name,
                Price = entity.Price,
            };

            await rabbitMqPublisher.Publish(message, "catalog-updates");
            uow.ProductRepository.Update(entity);
            await uow.SaveChangesAsync();
        }
    }
}
