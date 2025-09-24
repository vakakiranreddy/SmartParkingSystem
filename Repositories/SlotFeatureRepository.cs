using Microsoft.EntityFrameworkCore;
using SmartParkingSystem.Data;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Repositories
{
    public class SlotFeatureRepository : ISlotFeatureRepository
    {
        private readonly ParkingDbContext _context;

        public SlotFeatureRepository(ParkingDbContext context)
        {
            _context = context;
        }

        public async Task<SlotFeature> AddAsync(SlotFeature slotFeature)
        {
            try
            {
                await _context.SlotFeatures.AddAsync(slotFeature);
                await _context.SaveChangesAsync();
                return slotFeature;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding slot-feature assignment.", ex);
            }
        }

        public async Task<bool> DeleteAsync(int slotId, int featureId)
        {
            try
            {
                var entity = await _context.SlotFeatures
                    .FirstOrDefaultAsync(sf => sf.SlotId == slotId && sf.FeatureId == featureId);

                if (entity == null)
                    return false;

                _context.SlotFeatures.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting slot-feature for Slot {slotId} and Feature {featureId}.", ex);
            }
        }

        public async Task<IEnumerable<SlotFeature>> GetBySlotIdAsync(int slotId)
        {
            try
            {
                return await _context.SlotFeatures
                    .Include(sf => sf.Feature)
                    .Where(sf => sf.SlotId == slotId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching features for slot Id {slotId}.", ex);
            }
        }

        public async Task<IEnumerable<SlotFeature>> GetByFeatureIdAsync(int featureId)
        {
            try
            {
                return await _context.SlotFeatures
                    .Include(sf => sf.ParkingSlot)
                    .Where(sf => sf.FeatureId == featureId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching slots for feature Id {featureId}.", ex);
            }
        }

        public async Task<SlotFeature> GetSlotFeatureAsync(int slotId, int featureId)
        {
            try
            {
                return await _context.SlotFeatures
                    .FirstOrDefaultAsync(sf => sf.SlotId == slotId && sf.FeatureId == featureId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching slot-feature link for slot {slotId} and feature {featureId}.", ex);
            }
        }

        public async Task<bool> SlotFeatureExistsAsync(int slotId, int featureId)
        {
            try
            {
                return await _context.SlotFeatures
                    .AnyAsync(sf => sf.SlotId == slotId && sf.FeatureId == featureId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if slot-feature link exists for slot {slotId} and feature {featureId}.", ex);
            }
        }
    }
}
