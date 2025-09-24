using SmartParkingSystem.Models;

namespace SmartParkingSystem.DTOs.ParkingSession
{
    public class ParkingSessionResponseDto
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }
        public string VehicleLicensePlate { get; set; }

        public int SlotId { get; set; }
        public string SlotNumber { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; }

        // Planned entry time (also real entry if guard doesn’t update later)
        public DateTime EntryTime { get; set; }

        // Planned exit time (also real exit if guard doesn’t update later)
        public DateTime? ExitTime { get; set; }

        public SessionStatus Status { get; set; }
        public decimal ParkingFee { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
}
