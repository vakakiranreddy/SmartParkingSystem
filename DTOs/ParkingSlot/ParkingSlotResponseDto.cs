
namespace SmartParkingSystem.DTOs.ParkingSlot
{
    public class ParkingSlotResponseDto
    {
        public int Id { get; set; }
        public string SlotNumber { get; set; }
        public string Floor { get; set; }
        public string Section { get; set; }
        public string? SlotImageBase64 { get; set; }  
        public bool IsOccupied { get; set; }
        public bool IsActive { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime? NextAvailableTime { get; set; }
    }
}