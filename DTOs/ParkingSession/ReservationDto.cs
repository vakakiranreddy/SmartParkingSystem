using SmartParkingSystem.Models;

namespace SmartParkingSystem.DTOs.ParkingSession
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public string VehicleLicensePlate { get; set; }
        public string SlotNumber { get; set; }

        public DateTime PlannedEntryTime { get; set; }   // IST converted
        public DateTime? PlannedExitTime { get; set; }   // IST converted

        public SessionStatus Status { get; set; }
        public decimal EstimatedFee { get; set; }
    }
}
