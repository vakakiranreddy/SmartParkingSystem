using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Repositories
{
    public interface IVehicleRepository : IBaseRepository<Vehicle>
    {
        Task<IEnumerable<Vehicle>> GetByOwnerIdAsync(int ownerId);
        Task<Vehicle> GetByLicensePlateAsync(string licensePlate);
        Task<bool> LicensePlateExistsAsync(string licensePlate);
    }
}
