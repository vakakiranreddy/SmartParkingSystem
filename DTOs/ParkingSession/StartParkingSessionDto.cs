using System.ComponentModel.DataAnnotations;

namespace SmartParkingSystem.DTOs.ParkingSession
{
    public class StartParkingSessionDto
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int SlotId { get; set; }
    }
}
