using SmartParkingSystem.DTOs.SlotFeature;

namespace SmartParkingSystem.Interfaces.Services
{
    public interface ISlotFeatureService
    {
        Task<SlotFeatureResponseDto> AssignFeatureToSlotAsync(AssignSlotFeatureDto assignDto);
        Task<bool> RemoveFeatureFromSlotAsync(RemoveSlotFeatureDto removeDto);
        Task<IEnumerable<SlotFeatureResponseDto>> GetSlotFeaturesAsync(int slotId);
        Task<IEnumerable<SlotFeatureResponseDto>> GetFeatureAssignmentsAsync(int featureId);
    }
}
