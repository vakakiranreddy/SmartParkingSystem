using SmartParkingSystem.DTOs.ParkingRate;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Services
{
    public interface IParkingRateService
    {
        // Basic CRUD
        Task<ParkingRateResponseDto> GetByIdAsync(int id);
        Task<IEnumerable<ParkingRateResponseDto>> GetAllAsync();
        Task<ParkingRateResponseDto> CreateAsync(CreateParkingRateDto createDto);
        Task<ParkingRateResponseDto> UpdateAsync(int id, UpdateParkingRateDto updateDto);
        Task<bool> DeleteAsync(int id);

        // Rate-specific methods
        Task<ParkingRateResponseDto> GetRateByVehicleTypeAsync(VehicleType vehicleType);
        Task<IEnumerable<ParkingRateResponseDto>> GetActiveRatesAsync();
    }
}
