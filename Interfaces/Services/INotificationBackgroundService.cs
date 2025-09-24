using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Services
{
    public interface INotificationBackgroundService
    {
        Task SendReservationRemindersAsync();
        Task SendOverdueNotificationsAsync();
        Task SendSessionNotificationAsync(int sessionId, NotificationType notificationType);
    }
}
