using Microsoft.EntityFrameworkCore;
using SmartParkingSystem.Data;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Repositories
{
    public class VehicleRepository : BaseRepository<Vehicle>, IVehicleRepository
    {
        private readonly ParkingDbContext _context;

        public VehicleRepository(ParkingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vehicle>> GetByOwnerIdAsync(int ownerId)
        {
            try
            {
                return await _context.Set<Vehicle>()
                    .Where(v => v.OwnerId == ownerId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching vehicles for owner Id {ownerId}.", ex);
            }
        }

        public async Task<Vehicle> GetByLicensePlateAsync(string licensePlate)
        {
            try
            {
                return await _context.Set<Vehicle>()
                    .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching vehicle with license plate {licensePlate}.", ex);
            }
        }

        public async Task<bool> LicensePlateExistsAsync(string licensePlate)
        {
            try
            {
                return await _context.Set<Vehicle>()
                    .AnyAsync(v => v.LicensePlate == licensePlate);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if license plate {licensePlate} exists.", ex);
            }
        }
    }
}
