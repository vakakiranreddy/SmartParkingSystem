using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Repositories
{
    public interface IGuestRepository
    {
        Task<Guest> GetByIdAsync(int id);
        Task<IEnumerable<Guest>> GetAllAsync();
        Task<Guest> AddAsync(Guest guest);
        Task<Guest> UpdateAsync(Guest guest);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<Guest> GetByLicensePlateAsync(string licensePlate);
        Task<bool> LicensePlateExistsAsync(string licensePlate);
    }
}