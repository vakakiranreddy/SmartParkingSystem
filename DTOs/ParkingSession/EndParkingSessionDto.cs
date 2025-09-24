using System.ComponentModel.DataAnnotations;

namespace SmartParkingSystem.DTOs.ParkingSession
{
    public class EndParkingSessionDto
    {
        [Required]
        public int SessionId { get; set; }

        public DateTime? ExitTime { get; set; }
    }
}
