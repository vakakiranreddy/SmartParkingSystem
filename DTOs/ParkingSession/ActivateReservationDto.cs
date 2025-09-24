using System.ComponentModel.DataAnnotations;

namespace SmartParkingSystem.DTOs.ParkingSession
{
    public class ActivateReservationDto
    {
        [Required]
        public int ReservationId { get; set; }

        // Actual arrival time (usually DateTime.UtcNow)
        public DateTime? ActualEntryTime { get; set; }
    }
}
