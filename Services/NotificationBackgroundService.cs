using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartParkingSystem.DTOs.EmailNotification;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Models;
using System.Net;
using System.Net.Mail;

namespace SmartParkingSystem.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public NotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<NotificationBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var nowUtc = DateTime.UtcNow;
                    var nowIst = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, ist);

                    await ProcessNotificationsAsync();

                    // Calculate delay until the next full minute
                    var nextCheck = nowIst.AddMinutes(1);
                    nextCheck = new DateTime(nextCheck.Year, nextCheck.Month, nextCheck.Day, nextCheck.Hour, nextCheck.Minute, 0);

                    var delay = nextCheck - nowIst;
                    if (delay.TotalMilliseconds > 0)
                        await Task.Delay(delay, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in notification background service");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
        }


        //private async Task ProcessNotificationsAsync()
        //{
        //    using var scope = _serviceProvider.CreateScope();
        //    var sessionRepo = scope.ServiceProvider.GetRequiredService<IParkingSessionRepository>();

        //    var now = DateTime.UtcNow;
        //    var reminderWindow = now.AddMinutes(10);
        //    var reminderWindowEnd = now.AddMinutes(11);

        //    var reservedSessions = await sessionRepo.GetReservationsAsync();
        //    var activeSessions = await sessionRepo.GetActiveSessionsAsync();

        //    // Reservation reminders
        //    foreach (var session in reservedSessions
        //        .Where(s => s.Status == SessionStatus.Reserved &&
        //                    s.EntryTime >= reminderWindow &&
        //                    s.EntryTime < reminderWindowEnd))
        //    {
        //        await SendSessionEmailAsync(session, NotificationType.Reminder);
        //    }

        //    // Exit reminders
        //    foreach (var session in activeSessions
        //        .Where(s => s.ExitTime.HasValue &&
        //                    s.ExitTime.Value >= reminderWindow &&
        //                    s.ExitTime.Value < reminderWindowEnd))
        //    {
        //        await SendSessionEmailAsync(session, NotificationType.ExitReminder);
        //    }

        //    // Overdue notifications
        //    foreach (var session in activeSessions
        //        .Where(s => s.ExitTime.HasValue && now > s.ExitTime.Value.AddMinutes(15)))
        //    {
        //        await SendSessionEmailAsync(session, NotificationType.Overdue);
        //    }
        //}

        private async Task ProcessNotificationsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var sessionRepo = scope.ServiceProvider.GetRequiredService<IParkingSessionRepository>();

            var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var nowIst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ist);

            var reservedSessions = await sessionRepo.GetReservationsAsync();
            var activeSessions = await sessionRepo.GetActiveSessionsAsync();

            // ----------------------
            // Reservation reminders (10 minutes before entry)
            // ----------------------
            foreach (var session in reservedSessions
                .Where(s => s.Status == SessionStatus.Reserved &&
                            s.EntryTime != null))
            {
                var entryIst = TimeZoneInfo.ConvertTimeFromUtc(session.EntryTime, ist);
                var minutesToEntry = (entryIst - nowIst).TotalMinutes;

                if (minutesToEntry <= 10 && minutesToEntry > 0 && !session.ReminderSent)
                {
                    await SendSessionEmailAsync(session, NotificationType.Reminder);
                    session.ReminderSent = true;
                    await sessionRepo.UpdateAsync(session);
                }
            }

            // ----------------------
            // Exit reminders (10 minutes before exit)
            // ----------------------
            foreach (var session in activeSessions
                .Where(s => s.ExitTime.HasValue))
            {
                var exitIst = TimeZoneInfo.ConvertTimeFromUtc(session.ExitTime.Value, ist);
                var minutesToExit = (exitIst - nowIst).TotalMinutes;

                if (minutesToExit <= 10 && minutesToExit > 9)
                {
                    await SendSessionEmailAsync(session, NotificationType.ExitReminder);
                }
            }

            // ----------------------
            // Overdue notifications (15 minutes past exit)
            // ----------------------
            foreach (var session in activeSessions
                .Where(s => s.ExitTime.HasValue))
            {
                var exitIst = TimeZoneInfo.ConvertTimeFromUtc(session.ExitTime.Value, ist);
                if ((nowIst - exitIst).TotalMinutes > 15)
                {
                    await SendSessionEmailAsync(session, NotificationType.Overdue);
                }
            }
        }






        public async Task SendSessionEmailAsync(ParkingSession session, NotificationType type)
        {
            try
            {
                // Ensure related entities are loaded
                if (session.User == null || session.Vehicle == null || session.ParkingSlot == null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var sessionRepo = scope.ServiceProvider.GetRequiredService<IParkingSessionRepository>();
                    session = await sessionRepo.GetSessionWithDetailsAsync(session.Id);
                    if (session?.User == null) return;
                }

                var (subject, message) = GenerateEmailContent(session, type);

                // SMTP settings from appsettings.json
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = emailSettings["SmtpHost"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"]);
                var fromEmail = emailSettings["FromEmail"];
                var fromName = emailSettings["FromName"];
                var password = emailSettings["Password"];
                var enableSsl = bool.Parse(emailSettings["EnableSsl"]);

                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(fromEmail, password),
                    EnableSsl = enableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = false
                };
                mailMessage.To.Add(session.User.Email);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent: {Type} to {Email}", type, session.User.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send {Type} email for session {SessionId}", type, session.Id);
            }
        }

        private (string subject, string message) GenerateEmailContent(ParkingSession session, NotificationType type)
        {
            var userName = $"{session.User.FirstName} {session.User.LastName}";
            var slotNumber = session.ParkingSlot.SlotNumber;
            var licensePlate = session.Vehicle.LicensePlate;

            var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var entry = TimeZoneInfo.ConvertTimeFromUtc(session.EntryTime, ist);
            var exit = session.ExitTime.HasValue
                ? TimeZoneInfo.ConvertTimeFromUtc(session.ExitTime.Value, ist)
                : (DateTime?)null;

            return type switch
            {
                NotificationType.Reservation => ("Parking Slot Reserved",
                    $"Dear {userName},\nYour parking slot {slotNumber} is reserved.\nEntry: {entry:dd-MM-yyyy HH:mm} IST\nExit: {exit:dd-MM-yyyy HH:mm} IST"),
                NotificationType.Reminder => ("Parking Reservation Reminder",
                    $"Dear {userName},\nReminder: Your parking session starts soon at {entry:HH:mm} IST in slot {slotNumber}."),
                NotificationType.Entry => ("Parking Session Started",
                    $"Dear {userName},\nYour parking session has started at {entry:HH:mm} IST."),
                NotificationType.ExitReminder => ("Exit Reminder",
                    $"Dear {userName},\nYour parking session will end soon at {exit:HH:mm} IST."),
                NotificationType.PaymentReminder => ("Payment Reminder",
                    $"Dear {userName},\nPlease complete your payment of ₹{session.ParkingFee:F2} for session {session.Id}."),
                NotificationType.Overdue => ("Overdue Parking Notice",
                    $"Dear {userName},\nYour session {session.Id} has exceeded the planned exit time."),
                NotificationType.Exit => ("Parking Session Completed",
                    $"Dear {userName},\nYour parking session has been completed successfully."),
                NotificationType.Payment => ("Payment Confirmation",
                    $"Dear {userName},\nYour payment of ₹{session.ParkingFee:F2} has been processed."),
                _ => ("Smart Parking Notification", "Notification regarding your parking session.")
            };
        }
    }
}
