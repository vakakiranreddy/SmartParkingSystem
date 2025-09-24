using System.ComponentModel.DataAnnotations;

namespace SmartParkingSystem.DTOs.ParkingSlot
{
    public class UpdateParkingSlotDto
    {
        [Required]
        [StringLength(10)]
        public string SlotNumber { get; set; }

        [StringLength(10)]
        public string Floor { get; set; }

        [StringLength(10)]
        public string Section { get; set; }

        [Url]
        public string SlotImageUrl { get; set; }

        public bool IsActive { get; set; }
    }
}
