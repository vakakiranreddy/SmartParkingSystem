using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartParkingSystem.Models
{
    public class SlotFeature
    {
      

        [Required]
        public int SlotId { get; set; }

        [Required]
        public int FeatureId { get; set; }

        public bool IsActive { get; set; } = true;

       
        [ForeignKey("SlotId")]
        public ParkingSlot ParkingSlot { get; set; }

        [ForeignKey("FeatureId")]
        public Feature Feature { get; set; }
    }
}