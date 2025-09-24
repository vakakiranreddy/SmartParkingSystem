using Microsoft.EntityFrameworkCore;
using SmartParkingSystem.Data;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Repositories
{
    public class ParkingRateRepository : BaseRepository<ParkingRate>, IParkingRateRepository
    {
        private readonly ParkingDbContext _context;

        public ParkingRateRepository(ParkingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ParkingRate> GetByVehicleTypeAsync(VehicleType vehicleType)
        {
            try
            {
                return await _context.Set<ParkingRate>()
                    .FirstOrDefaultAsync(r => r.VehicleType == vehicleType && r.IsActive);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching rate for vehicle type {vehicleType}.", ex);
            }
        }

        public async Task<IEnumerable<ParkingRate>> GetActiveRatesAsync()
        {
            try
            {
                return await _context.Set<ParkingRate>()
                    .Where(r => r.IsActive)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching active parking rates.", ex);
            }
        }
    }
}
