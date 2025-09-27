//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using Microsoft.EntityFrameworkCore;

//namespace SmartParkingSystem.Models
//{
//    [Index(nameof(SlotNumber), IsUnique = true)]
//    public class ParkingSlot
//    {
//        [Key]
//        public int Id { get; set; }

//        [Required]
//        [StringLength(10)]
//        public string SlotNumber { get; set; }

//        [StringLength(10)]
//        public string Floor { get; set; }

//        [StringLength(10)]
//        public string Section { get; set; }

//        [StringLength(255)]
//        [Url]
//        public string SlotImageUrl { get; set; }

//        public bool IsOccupied { get; set; } = false;

//        public bool IsActive { get; set; } = true;


//        public ICollection<ParkingSession> ParkingSessions { get; set; } = new List<ParkingSession>();

//        public ICollection<SlotFeature> SlotFeatures { get; set; } = new List<SlotFeature>();
//    }
//}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SmartParkingSystem.Models
{
    [Index(nameof(SlotNumber), IsUnique = true)]
    public class ParkingSlot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string SlotNumber { get; set; }

        [StringLength(10)]
        public string Floor { get; set; }

        [StringLength(10)]
        public string Section { get; set; }

        public byte[]? SlotImage { get; set; }

        public bool IsOccupied { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public ICollection<ParkingSession> ParkingSessions { get; set; } = new List<ParkingSession>();

        public ICollection<SlotFeature> SlotFeatures { get; set; } = new List<SlotFeature>();
    }
}