using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Repositories
{
    public interface IParkingRateRepository : IBaseRepository<ParkingRate>
    {
        Task<ParkingRate> GetByVehicleTypeAsync(VehicleType vehicleType);
        Task<IEnumerable<ParkingRate>> GetActiveRatesAsync();
    }
}
