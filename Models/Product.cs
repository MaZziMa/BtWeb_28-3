using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab3.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Please enter a positive price")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Please specify a category")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }
        
        public List<ProductImage>? ProductImages { get; set; }
    }
}

