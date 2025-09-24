namespace SmartParkingSystem.DTOs.Feature
{
    public class FeatureResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public decimal PriceModifier { get; set; }
        public bool IsActive { get; set; }
    }
}
