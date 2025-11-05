using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }


        [Required]
        [MaxLength(50)]
        public required string Name { get; set; }


        // optional, can contain html
        public string? Description { get; set; }


        [Url]
        public string? Image { get; set; }


        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }


        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }


        [Required]
        [Range(0, int.MaxValue)]
        public int Amount { get; set; }
    }
}
