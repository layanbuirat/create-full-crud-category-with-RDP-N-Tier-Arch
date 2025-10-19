using System.ComponentModel.DataAnnotations;

namespace KASHOP.DTO.Requests
{
    public class UserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}