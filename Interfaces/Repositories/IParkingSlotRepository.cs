using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Repositories
{
    public interface IParkingSlotRepository : IBaseRepository<ParkingSlot>
    {
        Task<IEnumerable<ParkingSlot>> GetAvailableSlotsAsync();
        Task<IEnumerable<ParkingSlot>> GetByFloorAsync(string floor);
        Task<IEnumerable<ParkingSlot>> GetBySectionAsync(string section);
        Task<IEnumerable<ParkingSlot>> SearchSlotsAsync(string floor, string section, bool? isOccupied);
        Task<ParkingSlot> GetSlotWithFeaturesAsync(int slotId);
        Task<bool> SlotNumberExistsAsync(string slotNumber);

        // Feature-based search
        Task<IEnumerable<ParkingSlot>> GetSlotsByFeatureAsync(int featureId);
        Task<IEnumerable<ParkingSlot>> GetSlotsWithFeaturesAsync(List<int> featureIds);
        Task<IEnumerable<ParkingSlot>> GetAvailableSlotsWithFeaturesAsync(List<int> featureIds);
    }
}
