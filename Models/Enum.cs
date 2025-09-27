//using System.Text.Json.Serialization;

//namespace SmartParkingSystem.Models
//{
//    //public enum UserRole
//    //{
//    //    Admin,
//    //    User,
//    //    Guard
//    //}
//    [JsonConverter(typeof(JsonStringEnumConverter))]
//    public enum UserRole
//    {
//        Admin = 0,
//        User = 1,
//        Guard = 2
//    }
//    public enum VehicleType
//    {
//        Car,
//        Motorcycle,
//        Truck,
//        SUV,
//        Van
//    }

//    public enum SessionStatus
//    {

//        Reserved,
//        Active,
//        Completed,
//        Cancelled,
//        Expired
//    }

//    public enum PaymentStatus
//    {
//        Pending,
//        Paid,
//        Cancelled
//    }

//    public enum NotificationType
//    {
//        Reservation,
//        Reminder,
//        Entry,
//        ExitReminder,
//        PaymentReminder,
//        Overdue,
//        Exit,
//        Payment
//    }


//    public enum BroadcastNotificationType
//    {
//        SystemAnnouncement,
//        MaintenanceAlert,
//        PromotionalOffer,
//        PolicyUpdate,
//        EmergencyAlert
//    }

//    public enum EmailStatus
//    {
//        Pending = 0,
//        Sent = 1,
//        Failed = 2
//    }

//}

using System.Text.Json.Serialization;

namespace SmartParkingSystem.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        Admin = 0,
        User = 1,
        Guard = 2
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum VehicleType
    {
        Car,
        Motorcycle,
        Truck,
        SUV,
        Van
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SessionStatus
    {
        Reserved,
        Active,
        Completed,
        Cancelled,
        Expired
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Cancelled
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum NotificationType
    {
        Reservation,
        Reminder,
        Entry,
        ExitReminder,
        PaymentReminder,
        Overdue,
        Exit,
        Payment,
        General
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BroadcastNotificationType
    {
        SystemAnnouncement,
        MaintenanceAlert,
        PromotionalOffer,
        PolicyUpdate,
        EmergencyAlert
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EmailStatus
    {
        Pending = 0,
        Sent = 1,
        Failed = 2
    }
}