using System.ComponentModel.DataAnnotations;

//namespace SmartParkingSystem.DTOs.ParkingSlot
//{
//    public class CreateParkingSlotDto
//    {
//        [Required]
//        [StringLength(10)]
//        public string SlotNumber { get; set; }

//        [StringLength(10)]
//        public string Floor { get; set; }

//        [StringLength(10)]
//        public string Section { get; set; }

//        public byte[]? SlotImage { get; set; }
//    }

//}

namespace SmartParkingSystem.DTOs.ParkingSlot
{
    public class CreateParkingSlotDto
    {
        [Required]
        [StringLength(10)]
        public string SlotNumber { get; set; }

        [StringLength(10)]
        public string Floor { get; set; }

        [StringLength(10)]
        public string Section { get; set; }

        public string? SlotImageBase64 { get; set; }  // CHANGE FROM byte[]? SlotImage
    }
}