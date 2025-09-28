using System.ComponentModel.DataAnnotations;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.DTOs.ParkingSession
{
    public class StartGuestSessionDto
    {
        [Required]
        public int SlotId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; }

        [Required]
        public VehicleType VehicleType { get; set; }

        [StringLength(30)]
        public string Brand { get; set; }

        [StringLength(30)]
        public string Model { get; set; }

        [StringLength(20)]
        public string Color { get; set; }
    }
}