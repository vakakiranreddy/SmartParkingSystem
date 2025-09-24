using SmartParkingSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartParkingSystem.DTOs.User
{
    public class UpdateUserRoleDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public UserRole Role { get; set; }
    }

}
