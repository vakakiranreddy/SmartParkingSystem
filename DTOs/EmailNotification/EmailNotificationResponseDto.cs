using SmartParkingSystem.Models;

namespace SmartParkingSystem.DTOs.EmailNotification
{
    public class EmailNotificationResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int ParkingSessionId { get; set; }
        public string EmailAddress { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public NotificationType NotificationType { get; set; }
        public EmailStatus Status { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
