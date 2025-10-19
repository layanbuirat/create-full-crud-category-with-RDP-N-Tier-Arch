using System.ComponentModel.DataAnnotations;

namespace KASHOP.DTO.Requests
{
    public class BrandRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
        
        public IFormFile? LogoFile { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}