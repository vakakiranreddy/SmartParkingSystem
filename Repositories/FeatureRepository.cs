using Microsoft.EntityFrameworkCore;
using SmartParkingSystem.Data;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Repositories
{
    public class FeatureRepository : BaseRepository<Feature>, IFeatureRepository
    {
        private readonly ParkingDbContext _context;

        public FeatureRepository(ParkingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Feature>> GetActiveFeaturesAsync()
        {
            try
            {
                return await _context.Set<Feature>()
                    .Where(f => f.IsActive)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching active features.", ex);
            }
        }

        public async Task<Feature> GetByNameAsync(string name)
        {
            try
            {
                return await _context.Set<Feature>()
                    .FirstOrDefaultAsync(f => f.Name == name);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching feature with name {name}.", ex);
            }
        }

        public async Task<bool> NameExistsAsync(string name)
        {
            try
            {
                return await _context.Set<Feature>()
                    .AnyAsync(f => f.Name == name);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if feature name {name} exists.", ex);
            }
        }
    }
}
