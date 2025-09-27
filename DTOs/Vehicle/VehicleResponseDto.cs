using SmartParkingSystem.Models;

namespace SmartParkingSystem.DTOs.Vehicle
{
    public class VehicleResponseDto
    {
        public int Id { get; set; }
        public string LicensePlate { get; set; }
        public VehicleType VehicleType { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public string VehicleImageBase64 { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
