using System.ComponentModel.DataAnnotations;

namespace SmartParkingSystem.DTOs.User
{
    public class UpdateUserDto
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [Phone]
        [StringLength(15)]
        public string PhoneNumber { get; set; }

        [Url]
        public string ProfileImageUrl { get; set; }
    }
}
