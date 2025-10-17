using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartParkingSystem.Models
{
    public class ParkingSession
    {
        [Key]
        public int Id { get; set; }


        public int? VehicleId { get; set; }

        [Required]
        public int SlotId { get; set; }

     
        public int? UserId { get; set; }

     
        public int? GuestId { get; set; }

        public bool EntryReminderSent { get; set; } = false;
        public bool ExitReminderSent { get; set; } = false;
        public bool OverdueReminderSent { get; set; } = false;

        public DateTime? ReservedTime { get; set; }
        public DateTime EntryTime { get; set; }

        public DateTime? ExitTime { get; set; }

        [Required]
        public SessionStatus Status { get; set; } = SessionStatus.Active;

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ParkingFee { get; set; } = 0;

        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;


        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }

        [ForeignKey("SlotId")]
        public ParkingSlot ParkingSlot { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("GuestId")]
        public Guest Guest { get; set; }

        public ICollection<EmailNotification> EmailNotifications { get; set; } = new List<EmailNotification>();
    }
}