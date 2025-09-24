using SmartParkingSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartParkingSystem.DTOs.BroadcastNotification
{
    public class UpdateBroadcastNotificationDto
    {
        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        [Required]
        [StringLength(2000)]
        public string Message { get; set; }

        [Required]
        public BroadcastNotificationType NotificationType { get; set; }

        public UserRole? TargetRole { get; set; }
        public bool IsActive { get; set; }
    }
}
