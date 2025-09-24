namespace SmartParkingSystem.Models
{
    public enum UserRole
    {
        Admin,
        User,
        Guard
    }

    public enum VehicleType
    {
        Car,
        Motorcycle,
        Truck,
        SUV,
        Van
    }

    public enum SessionStatus
    {

        Reserved,
        Active,
        Completed,
        Cancelled,
        Expired
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        Cancelled
    }

    public enum NotificationType
    {
        Reservation,
        Reminder,
        Entry,
        ExitReminder,
        PaymentReminder,
        Overdue,
        Exit,
        Payment
    }


    public enum BroadcastNotificationType
    {
        SystemAnnouncement,
        MaintenanceAlert,
        PromotionalOffer,
        PolicyUpdate,
        EmergencyAlert
    }

    public enum EmailStatus
    {
        Pending = 0,
        Sent = 1,
        Failed = 2
    }

}