using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Repositories
{
    public interface IEmailNotificationRepository : IBaseRepository<EmailNotification>
    {
        Task<IEnumerable<EmailNotification>> GetByUserIdAsync(int userId);
        Task<IEnumerable<EmailNotification>> GetPendingNotificationsAsync();
        Task<IEnumerable<EmailNotification>> GetByStatusAsync(EmailStatus status);
    }
}
