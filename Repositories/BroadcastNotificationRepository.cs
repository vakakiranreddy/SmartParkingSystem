using Microsoft.EntityFrameworkCore;
using SmartParkingSystem.Data;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Repositories
{
    public class BroadcastNotificationRepository : BaseRepository<BroadcastNotification>, IBroadcastNotificationRepository
    {
        private readonly ParkingDbContext _context;

        public BroadcastNotificationRepository(ParkingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BroadcastNotification>> GetByTargetRoleAsync(UserRole? targetRole)
        {
            try
            {
                var query = _context.Set<BroadcastNotification>().AsQueryable();

                if (targetRole.HasValue)
                    query = query.Where(b => b.TargetRole == targetRole.Value);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching broadcast notifications for target role {targetRole}.", ex);
            }
        }

        public async Task<IEnumerable<BroadcastNotification>> GetPendingBroadcastsAsync()
        {
            try
            {
                return await _context.Set<BroadcastNotification>()
                    .Where(b => b.Status == EmailStatus.Pending && b.IsActive)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching pending broadcast notifications.", ex);
            }
        }

        public async Task<IEnumerable<BroadcastNotification>> GetActiveNotificationsAsync()
        {
            try
            {
                return await _context.Set<BroadcastNotification>()
                    .Where(b => b.IsActive)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching active broadcast notifications.", ex);
            }
        }
    }
}
