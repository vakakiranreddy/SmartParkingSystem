using SmartParkingSystem.DTOs.EmailNotification;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Services
{
    public interface IEmailNotificationService
    {
        Task<EmailNotificationResponseDto> SendNotificationAsync(SendEmailNotificationDto notificationDto);
        Task<IEnumerable<EmailNotificationResponseDto>> GetUserNotificationsAsync(int userId);
        Task<bool> ProcessPendingEmailsAsync();
       
        Task<bool> SendGuestEmailAsync(string guestEmail, string guestName, string subject, string message);
    }
}

