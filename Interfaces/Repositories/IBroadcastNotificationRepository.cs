using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Repositories
{
    public interface IBroadcastNotificationRepository : IBaseRepository<BroadcastNotification>
    {
        Task<IEnumerable<BroadcastNotification>> GetByTargetRoleAsync(UserRole? targetRole);
        Task<IEnumerable<BroadcastNotification>> GetPendingBroadcastsAsync();
        Task<IEnumerable<BroadcastNotification>> GetActiveNotificationsAsync();
    }
}
