using System.ComponentModel.DataAnnotations;

namespace KASHOP.DTO.Requests
{
    public class ProductUpdateRequest
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Required]
        public int CategoryId { get; set; }
        
        public int? BrandId { get; set; }
        
        public IFormFile? ImageFile { get; set; }
        
        public bool IsFeatured { get; set; }
        
        public bool IsActive { get; set; }
    }
}