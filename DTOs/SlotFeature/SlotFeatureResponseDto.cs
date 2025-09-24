namespace SmartParkingSystem.DTOs.SlotFeature
{
    public class SlotFeatureResponseDto
    {
        public int SlotId { get; set; }
        public string SlotNumber { get; set; }
        public int FeatureId { get; set; }
        public string FeatureName { get; set; }
        public decimal PriceModifier { get; set; }
        public bool IsActive { get; set; }
    }
}
