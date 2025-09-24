using System.ComponentModel.DataAnnotations;

namespace SmartParkingSystem.DTOs.ParkingSession
{
    public class UpdateParkingFeeDto
    {
        [Required]
        public int SessionId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ParkingFee { get; set; }
    }
}
