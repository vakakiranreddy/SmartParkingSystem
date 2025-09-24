using SmartParkingSystem.DTOs.Vehicle;

namespace SmartParkingSystem.Interfaces.Services
{
    public interface IVehicleService
    {
        Task<VehicleResponseDto> GetByIdAsync(int id);
        Task<IEnumerable<VehicleResponseDto>> GetAllAsync();
        Task<VehicleResponseDto> CreateAsync(CreateVehicleDto createDto, int ownerId); // 👈 add this
        Task<VehicleResponseDto> UpdateAsync(int id, UpdateVehicleDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<VehicleResponseDto>> GetUserVehiclesAsync(int userId);
        Task<VehicleResponseDto> GetByLicensePlateAsync(string licensePlate);
    }
}
