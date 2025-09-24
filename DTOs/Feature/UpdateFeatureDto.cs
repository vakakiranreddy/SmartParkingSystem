using System.ComponentModel.DataAnnotations;

namespace SmartParkingSystem.DTOs.Feature
{
    public class UpdateFeatureDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        [Url]
        public string IconUrl { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PriceModifier { get; set; } = 0;

        public bool IsActive { get; set; }
    }
}
