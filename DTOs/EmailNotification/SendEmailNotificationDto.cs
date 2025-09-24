using SmartParkingSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartParkingSystem.DTOs.EmailNotification
{
    public class SendEmailNotificationDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int ParkingSessionId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string EmailAddress { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        [Required]
        [StringLength(2000)]
        public string Message { get; set; }

        [Required]
        public NotificationType NotificationType { get; set; }
    }
}
