using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartParkingSystem.Models
{
    [Index(nameof(LicensePlate), IsUnique = true)]
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

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

        [StringLength(255)]
        [Url]
        public string VehicleImageUrl { get; set; }

        [Required]
        public int OwnerId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

      
        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

        public ICollection<ParkingSession> ParkingSessions { get; set; } = new List<ParkingSession>();
    }
}