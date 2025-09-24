using SmartParkingSystem.DTOs.Feature;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Services
{
    public interface IFeatureService
    {
        // Basic CRUD
        Task<FeatureResponseDto> GetByIdAsync(int id);
        Task<IEnumerable<FeatureResponseDto>> GetAllAsync();
        Task<FeatureResponseDto> CreateAsync(CreateFeatureDto createDto);
        Task<FeatureResponseDto> UpdateAsync(int id, UpdateFeatureDto updateDto);
        Task<bool> DeleteAsync(int id);

        // Feature-specific methods
        Task<IEnumerable<FeatureResponseDto>> GetActiveFeaturesAsync();
    }
}
