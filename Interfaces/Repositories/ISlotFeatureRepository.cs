using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Repositories
{
    public interface ISlotFeatureRepository
    {
        Task<IEnumerable<SlotFeature>> GetBySlotIdAsync(int slotId);
        Task<IEnumerable<SlotFeature>> GetByFeatureIdAsync(int featureId);
        Task<SlotFeature> GetSlotFeatureAsync(int slotId, int featureId);
        Task<bool> SlotFeatureExistsAsync(int slotId, int featureId);
        Task<SlotFeature> AddAsync(SlotFeature slotFeature);
        Task<bool> DeleteAsync(int slotId, int featureId);

    }

}
