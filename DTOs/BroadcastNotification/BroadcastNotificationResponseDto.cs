using SmartParkingSystem.Models;

namespace SmartParkingSystem.DTOs.BroadcastNotification
{
    public class BroadcastNotificationResponseDto
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public BroadcastNotificationType NotificationType { get; set; }
        public EmailStatus Status { get; set; }
        public DateTime? SentAt { get; set; }
        public string ErrorMessage { get; set; }
        public UserRole? TargetRole { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
