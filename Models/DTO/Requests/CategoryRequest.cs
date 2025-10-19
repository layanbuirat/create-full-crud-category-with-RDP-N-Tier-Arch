using System.ComponentModel.DataAnnotations;

namespace KASHOP.DTO.Requests
{
    public class CategoryRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
        
        public IFormFile? ImageFile { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}