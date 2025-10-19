using System.ComponentModel.DataAnnotations;

namespace KASHOP.Models
{
    public class Brand
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public string LogoUrl { get; set; } = string.Empty;
        
        public string Website { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        public List<Product> Products { get; set; } = new List<Product>();
    }
}