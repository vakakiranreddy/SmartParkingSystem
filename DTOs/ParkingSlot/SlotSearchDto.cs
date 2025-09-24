namespace SmartParkingSystem.DTOs.ParkingSlot
{
    public class SlotSearchDto
    {
        public string Floor { get; set; }
        public string Section { get; set; }
        public bool? IsOccupied { get; set; }
    }
}
