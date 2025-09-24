using System.ComponentModel.DataAnnotations;

namespace SmartParkingSystem.DTOs.ParkingSession
{
    public class BookSlotDto
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int SlotId { get; set; }

        [Required]
        public DateTime PlannedEntryTime { get; set; } // User’s planned entry

        [Required]
        public DateTime PlannedExitTime { get; set; }  // User’s planned exit
    }
}
