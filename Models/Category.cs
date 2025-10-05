using System.ComponentModel.DataAnnotations;

namespace KASHOP.Models
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public List<Product> Products { get; set; } = new List<Product>();
    }
}