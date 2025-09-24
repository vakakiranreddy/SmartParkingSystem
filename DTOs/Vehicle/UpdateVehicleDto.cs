using SmartParkingSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartParkingSystem.DTOs.Vehicle
{
    public class UpdateVehicleDto
    {
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

        [Url]
        public string VehicleImageUrl { get; set; }
    }
}
