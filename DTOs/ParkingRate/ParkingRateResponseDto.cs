using SmartParkingSystem.Models;

namespace SmartParkingSystem.DTOs.ParkingRate
{
    public class ParkingRateResponseDto
    {
        public int Id { get; set; }
        public VehicleType VehicleType { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal DailyRate { get; set; }
        public bool IsActive { get; set; }
    }
}
