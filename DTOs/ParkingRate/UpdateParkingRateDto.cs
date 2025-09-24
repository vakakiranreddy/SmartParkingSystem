using SmartParkingSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartParkingSystem.DTOs.ParkingRate
{
    public class UpdateParkingRateDto
    {
        [Required]
        public VehicleType VehicleType { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal HourlyRate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal DailyRate { get; set; }

        public bool IsActive { get; set; }
    }
}
