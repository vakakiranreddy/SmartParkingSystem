using SmartParkingSystem.DTOs.ParkingSlot;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Services
{
    public interface IParkingSlotService
    {
        // Basic CRUD
        Task<ParkingSlotResponseDto> GetByIdAsync(int id);
        Task<IEnumerable<ParkingSlotResponseDto>> GetAllAsync();
        Task<ParkingSlotResponseDto> CreateAsync(CreateParkingSlotDto createDto);
        Task<ParkingSlotResponseDto> UpdateAsync(int id, UpdateParkingSlotDto updateDto);
        Task<bool> DeleteAsync(int id);

        // Slot-specific methods
        Task<IEnumerable<ParkingSlotResponseDto>> GetAvailableSlotsAsync();
        Task<IEnumerable<ParkingSlotResponseDto>> SearchSlotsAsync(SlotSearchDto searchDto);
        Task<IEnumerable<ParkingSlotResponseDto>> GetSlotsByFloorAsync(string floor);
        Task<ParkingSlotResponseDto> GetSlotWithFeaturesAsync(int slotId);
        Task<IEnumerable<ParkingSlotResponseDto>> GetSlotsByFeatureAsync(int featureId);
        Task<IEnumerable<ParkingSlotResponseDto>> GetSlotsWithFeaturesAsync(List<int> featureIds);
        Task<IEnumerable<ParkingSlotResponseDto>> GetAvailableSlotsWithFeaturesAsync(List<int> featureIds);
        Task<bool> BulkUpdateSlotStatusAsync(List<int> slotIds, bool isActive);
        Task<bool> BulkUpdateSlotOccupancyAsync(List<int> slotIds, bool isOccupied);
    }
}
