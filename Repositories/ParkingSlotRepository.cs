using Microsoft.EntityFrameworkCore;
using SmartParkingSystem.Data;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Repositories
{
    public class ParkingSlotRepository : BaseRepository<ParkingSlot>, IParkingSlotRepository
    {
        private readonly ParkingDbContext _context;

        public ParkingSlotRepository(ParkingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ParkingSlot>> GetAvailableSlotsAsync()
        {
            try
            {
                return await _context.Set<ParkingSlot>()
                    .Where(s => !s.IsOccupied && s.IsActive)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching available parking slots.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSlot>> GetByFloorAsync(string floor)
        {
            try
            {
                return await _context.Set<ParkingSlot>()
                    .Where(s => s.Floor == floor)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching slots on floor {floor}.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSlot>> GetBySectionAsync(string section)
        {
            try
            {
                return await _context.Set<ParkingSlot>()
                    .Where(s => s.Section == section)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching slots in section {section}.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSlot>> SearchSlotsAsync(string floor, string section, bool? isOccupied)
        {
            try
            {
                var query = _context.Set<ParkingSlot>().AsQueryable();

                if (!string.IsNullOrEmpty(floor))
                    query = query.Where(s => s.Floor == floor);

                if (!string.IsNullOrEmpty(section))
                    query = query.Where(s => s.Section == section);

                if (isOccupied.HasValue)
                    query = query.Where(s => s.IsOccupied == isOccupied.Value);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching parking slots.", ex);
            }
        }

        public async Task<ParkingSlot> GetSlotWithFeaturesAsync(int slotId)
        {
            try
            {
                return await _context.Set<ParkingSlot>()
                    .Include(s => s.SlotFeatures)
                        .ThenInclude(sf => sf.Feature)
                    .FirstOrDefaultAsync(s => s.Id == slotId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching slot with features for Id {slotId}.", ex);
            }
        }

        public async Task<bool> SlotNumberExistsAsync(string slotNumber)
        {
            try
            {
                return await _context.Set<ParkingSlot>()
                    .AnyAsync(s => s.SlotNumber == slotNumber);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if slot number {slotNumber} exists.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSlot>> GetSlotsByFeatureAsync(int featureId)
        {
            try
            {
                return await _context.Set<ParkingSlot>()
                    .Include(s => s.SlotFeatures)
                    .Where(s => s.SlotFeatures.Any(sf => sf.FeatureId == featureId))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching slots with feature Id {featureId}.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSlot>> GetSlotsWithFeaturesAsync(List<int> featureIds)
        {
            try
            {
                return await _context.Set<ParkingSlot>()
                    .Include(s => s.SlotFeatures)
                    .Where(s => featureIds.All(fid => s.SlotFeatures.Any(sf => sf.FeatureId == fid)))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching slots with all specified features.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSlot>> GetAvailableSlotsWithFeaturesAsync(List<int> featureIds)
        {
            try
            {
                return await _context.Set<ParkingSlot>()
                    .Include(s => s.SlotFeatures)
                    .Where(s => !s.IsOccupied && s.IsActive &&
                                featureIds.All(fid => s.SlotFeatures.Any(sf => sf.FeatureId == fid)))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching available slots with specified features.", ex);
            }
        }
    }
}
