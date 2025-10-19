using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }


        [Required]
        [MaxLength(50)]
        public string Name { get; set; }


        [Url]
        public string Image { get; set; }


        public int? ParentCategoryId { get; set; }
        public Category ParentCategory { get; set; }


        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
