using SmartParkingSystem.DTOs.EmailNotification;

namespace SmartParkingSystem.Interfaces.Services
{
    public interface IEmailNotificationService
    {
        Task<EmailNotificationResponseDto> SendNotificationAsync(SendEmailNotificationDto notificationDto);
        Task<IEnumerable<EmailNotificationResponseDto>> GetUserNotificationsAsync(int userId);
        Task<bool> ProcessPendingEmailsAsync();
    }
}
