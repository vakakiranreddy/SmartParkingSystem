using System.ComponentModel.DataAnnotations;

namespace SmartParkingSystem.DTOs.SlotFeature
{
    public class AssignSlotFeatureDto
    {
        [Required]
        public int SlotId { get; set; }

        [Required]
        public int FeatureId { get; set; }
    }
}
