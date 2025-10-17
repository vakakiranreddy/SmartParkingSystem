using SmartParkingSystem.DTOs.EmailNotification;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Interfaces.Services;
using SmartParkingSystem.Models;
using System.Net;
using System.Net.Mail;

namespace SmartParkingSystem.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IEmailNotificationRepository _emailRepository;
        private readonly IUserRepository _userRepository;
        private readonly IParkingSessionRepository _sessionRepository;
        private readonly IConfiguration _configuration;

        public EmailNotificationService(
            IEmailNotificationRepository emailRepository,
            IUserRepository userRepository,
            IParkingSessionRepository sessionRepository,
            IConfiguration configuration)
        {
            _emailRepository = emailRepository;
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            _configuration = configuration;
        }


        public async Task<EmailNotificationResponseDto> SendNotificationAsync(SendEmailNotificationDto notificationDto)
        {
            try
            {
                
                if (notificationDto.UserId <= 0)
                {
                    throw new ArgumentException("SendNotificationAsync should only be called for registered users. Use SendGuestEmailAsync for guests.");
                }

             
                var user = await _userRepository.GetByIdAsync(notificationDto.UserId);
                if (user == null)
                    throw new ArgumentException($"User with Id {notificationDto.UserId} not found.");

               
                ParkingSession session = null;
                if (notificationDto.ParkingSessionId > 0)
                {
                    session = await _sessionRepository.GetSessionWithDetailsAsync(notificationDto.ParkingSessionId);
                    if (session == null)
                        throw new ArgumentException($"Parking session with Id {notificationDto.ParkingSessionId} not found.");
                }

                
                if (session != null)
                {
                    (notificationDto.Subject, notificationDto.Message) = GenerateEmailContent(session, notificationDto.NotificationType);
                }

                
                var emailNotification = new EmailNotification
                {
                    UserId = notificationDto.UserId,
                    ParkingSessionId = notificationDto.ParkingSessionId,
                    EmailAddress = notificationDto.EmailAddress,
                    Subject = notificationDto.Subject,
                    Message = notificationDto.Message,
                    NotificationType = notificationDto.NotificationType,
                    Status = EmailStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                Console.WriteLine($"DEBUG: Saving notification for registered user {notificationDto.UserId}");
                var createdNotification = await _emailRepository.AddAsync(emailNotification);

                
                var emailSent = await SendEmailAsync(createdNotification);

                createdNotification.Status = emailSent ? EmailStatus.Sent : EmailStatus.Failed;
                if (emailSent) createdNotification.SentAt = DateTime.UtcNow;

                await _emailRepository.UpdateAsync(createdNotification);
                return await MapToEmailNotificationResponseDto(createdNotification);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error sending email notification.", ex);
            }
        }

    

        public async Task<bool> SendGuestEmailAsync(string guestEmail, string guestName, string subject, string message)
        {
            try
            {
                Console.WriteLine($"SendGuestEmail: Sending email to {guestEmail}");

                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = emailSettings["SmtpHost"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"]);
                var fromEmail = emailSettings["FromEmail"];
                var fromName = emailSettings["FromName"];
                var password = emailSettings["Password"];
                var enableSsl = bool.Parse(emailSettings["EnableSsl"]);

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = enableSsl,
                    Credentials = new NetworkCredential(fromEmail, password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(guestEmail);
                await client.SendMailAsync(mailMessage);

                Console.WriteLine($"SendGuestEmail: Email sent successfully to {guestEmail}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendGuestEmail ERROR: Failed to send email to {guestEmail}: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<EmailNotificationResponseDto>> GetUserNotificationsAsync(int userId)
        {
            try
            {
                var notifications = await _emailRepository.GetByUserIdAsync(userId);
                var notificationDtos = new List<EmailNotificationResponseDto>();

                foreach (var notification in notifications)
                {
                    var dto = await MapToEmailNotificationResponseDto(notification);
                    notificationDtos.Add(dto);
                }

                return notificationDtos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving notifications for user Id {userId}.", ex);
            }
        }

        public async Task<bool> ProcessPendingEmailsAsync()
        {
            try
            {
                var pendingEmails = await _emailRepository.GetPendingNotificationsAsync();
                var successCount = 0;

                foreach (var email in pendingEmails)
                {
                    var emailSent = await SendEmailAsync(email);
                    email.Status = emailSent ? EmailStatus.Sent : EmailStatus.Failed;
                    if (emailSent) email.SentAt = DateTime.UtcNow;

                    await _emailRepository.UpdateAsync(email);
                    if (emailSent) successCount++;
                }

                return successCount > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error processing pending emails.", ex);
            }
        }

        private async Task<bool> SendEmailAsync(EmailNotification emailNotification)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = emailSettings["SmtpHost"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"]);
                var fromEmail = emailSettings["FromEmail"];
                var fromName = emailSettings["FromName"];
                var password = emailSettings["Password"];
                var enableSsl = bool.Parse(emailSettings["EnableSsl"]);

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = enableSsl,
                    Credentials = new NetworkCredential(fromEmail, password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = emailNotification.Subject,
                    Body = emailNotification.Message,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(emailNotification.EmailAddress);
                await client.SendMailAsync(mailMessage);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email to {emailNotification.EmailAddress}: {ex}");
                return false;
            }
        }
       
       

        private async Task<EmailNotificationResponseDto> MapToEmailNotificationResponseDto(EmailNotification notification)
        {
            var dto = new EmailNotificationResponseDto
            {
                Id = notification.Id,
                UserId = notification.UserId ?? 0, 
                UserName = "",
                ParkingSessionId = notification.ParkingSessionId,
                EmailAddress = notification.EmailAddress,
                Subject = notification.Subject,
                Message = notification.Message,
                NotificationType = notification.NotificationType,
                Status = notification.Status,
                SentAt = notification.SentAt,
                CreatedAt = notification.CreatedAt
            };

            try
            {
                if (notification.UserId.HasValue && notification.UserId > 0)
                {
                    var user = await _userRepository.GetByIdAsync(notification.UserId.Value);
                    if (user != null)
                        dto.UserName = $"{user.FirstName} {user.LastName}";
                    else
                        dto.UserName = "Unknown User";
                }
                else
                {
                    dto.UserName = "Guest User";
                }
            }
            catch
            {
                dto.UserName = "Guest User";
            }

            return dto;
        }


        private (string subject, string message) GenerateEmailContent(ParkingSession session, NotificationType notificationType)
        {
            var userName = $"{session.User?.FirstName} {session.User?.LastName}";
            var slotNumber = session.ParkingSlot?.SlotNumber ?? "Unknown";
            var licensePlate = session.Vehicle?.LicensePlate ?? "Unknown";

            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            var reservedTimeIst = session.ReservedTime.HasValue
                ? TimeZoneInfo.ConvertTimeFromUtc(session.ReservedTime.Value, istTimeZone)
                : (DateTime?)null;

            var entryTimeIst = TimeZoneInfo.ConvertTimeFromUtc(session.EntryTime, istTimeZone);

            var exitTimeIst = session.ExitTime.HasValue
                ? TimeZoneInfo.ConvertTimeFromUtc(session.ExitTime.Value, istTimeZone)
                : (DateTime?)null;

            return notificationType switch
            {
                NotificationType.Reservation => (
                    "Parking Slot Reserved - Smart Parking System",
                    $"Dear {userName},\n\n" +
                    $"Your parking slot has been successfully reserved!\n\n" +
                    $"Details:\n" +
                    $"• Vehicle: {licensePlate}\n" +
                    $"• Slot: {slotNumber}\n" +
                    $"• Planned Entry: {entryTimeIst:dd-MM-yyyy HH:mm} IST\n" +
                    $"• Planned Exit: {exitTimeIst:dd-MM-yyyy HH:mm} IST\n" +
                    $"• Session ID: {session.Id}\n\n" +
                    $"Please arrive on time.\n\n" +
                    $"Thank you!"
                ),
                NotificationType.Entry => (
                    "Parking Session Started - Smart Parking System",
                    $"Dear {userName},\n\n" +
                    $"Your parking session has started.\n\n" +
                    $"• Vehicle: {licensePlate}\n" +
                    $"• Slot: {slotNumber}\n" +
                    $"• Entry Time: {entryTimeIst:dd-MM-yyyy HH:mm} IST\n" +
                    $"• Planned Exit: {exitTimeIst:dd-MM-yyyy HH:mm} IST\n" +
                    $"• Session ID: {session.Id}"
                ),
                NotificationType.Exit => (
                    "Parking Session Completed - Smart Parking System",
                    $"Dear {userName},\n\n" +
                    $"Your parking session has been completed.\n\n" +
                    $"• Vehicle: {licensePlate}\n" +
                    $"• Slot: {slotNumber}\n" +
                    $"• Entry Time: {entryTimeIst:dd-MM-yyyy HH:mm} IST\n" +
                    $"• Exit Time: {exitTimeIst:dd-MM-yyyy HH:mm} IST\n" +
                    $"• Total Fee: ₹{session.ParkingFee:F2}\n" +
                    $"• Session ID: {session.Id}"
                ),
                NotificationType.General => (
     "Welcome to Smart Parking System 🚗", // ← Add the subject
     $"Dear {userName},\n\n" +
     "🎉 Congratulations! Your account has been successfully registered in Smart Parking System.\n\n" +
     "Now you can:\n" +
     "✅ Reserve parking slots online\n" +
     "✅ Manage your vehicles\n" +
     "✅ Get reminders & payment updates\n\n" +
     "Thank you for joining us!\n" +
     "- Smart Parking System Team"
 ),
                _ => (
                    "Smart Parking System Notification",
                    $"Dear {userName},\n\n" +
                    $"This is a notification for your parking session {session.Id}."
                )
            };
        }
    }
}
