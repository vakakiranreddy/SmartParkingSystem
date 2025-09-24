namespace SmartParkingSystem.DTOs.ParkingSlot
{
    public class ParkingSlotResponseDto
    {
        public int Id { get; set; }
        public string SlotNumber { get; set; }
        public string Floor { get; set; }
        public string Section { get; set; }
        public string SlotImageUrl { get; set; }
        public bool IsOccupied { get; set; }              // Currently occupied
        public bool IsActive { get; set; }

        // NEW: Availability info
        public bool IsAvailable { get; set; }             // Available right now
        public DateTime? NextAvailableTime { get; set; }   // When it becomes available

    }
}
