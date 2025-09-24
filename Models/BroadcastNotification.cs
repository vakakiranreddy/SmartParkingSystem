using System.ComponentModel.DataAnnotations;

namespace SmartParkingSystem.Models
{
    public class BroadcastNotification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        [Required]
        [StringLength(2000)]
        public string Message { get; set; }

        [Required]
        public BroadcastNotificationType NotificationType { get; set; }

        [Required]
        public EmailStatus Status { get; set; } = EmailStatus.Pending;

        public DateTime? SentAt { get; set; }

        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        // Target audience filters (optional)
        public UserRole? TargetRole { get; set; } // null = all users

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
