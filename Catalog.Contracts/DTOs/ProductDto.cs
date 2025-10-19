using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Contracts.DTOs
{
    public record ProductDto(int Id, string Name, string Description, string Image, int CategoryId, decimal Price, int Amount);
    public record CreateProductDto(string Name, string Description, string Image, int CategoryId, decimal Price, int Amount);
    public record UpdateProductDto(int Id, string Name, string Description, string Image, int CategoryId, decimal Price, int Amount);
}
