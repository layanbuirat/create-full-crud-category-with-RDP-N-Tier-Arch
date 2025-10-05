using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KASHOP.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        
        public int StockQuantity { get; set; }
        
        public string ImageUrl { get; set; } = string.Empty;
        
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}