using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartParkingSystem.Models
{
    public class ParkingRate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public VehicleType VehicleType { get; set; }

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal HourlyRate { get; set; }

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal DailyRate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}