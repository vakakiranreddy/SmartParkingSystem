using Microsoft.Extensions.Configuration;
using SmartParkingSystem.DTOs.BroadcastNotification;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Interfaces.Services;
using SmartParkingSystem.Models;
using System.Net;
using System.Net.Mail;

namespace SmartParkingSystem.Services
{
    public class BroadcastNotificationService : IBroadcastNotificationService
    {
        private readonly IBroadcastNotificationRepository _broadcastRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailNotificationService _emailService;
        private readonly IConfiguration _configuration;

        public BroadcastNotificationService(
            IBroadcastNotificationRepository broadcastRepository,
            IUserRepository userRepository,
            IEmailNotificationService emailService,
            IConfiguration configuration)
        {
            _broadcastRepository = broadcastRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _configuration = configuration;
        }

        // Basic CRUD methods
        public async Task<BroadcastNotificationResponseDto> GetByIdAsync(int id)
        {
            try
            {
                var broadcast = await _broadcastRepository.GetByIdAsync(id);
                if (broadcast == null)
                    throw new ArgumentException($"Broadcast notification with Id {id} not found.");

                return MapToBroadcastNotificationResponseDto(broadcast);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving broadcast notification with Id {id}.", ex);
            }
        }

        public async Task<IEnumerable<BroadcastNotificationResponseDto>> GetAllAsync()
        {
            try
            {
                var broadcasts = await _broadcastRepository.GetAllAsync();
                return broadcasts.Select(MapToBroadcastNotificationResponseDto);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all broadcast notifications.", ex);
            }
        }

        public async Task<BroadcastNotificationResponseDto> CreateAsync(CreateBroadcastNotificationDto createDto)
        {
            try
            {
                var broadcast = new BroadcastNotification
                {
                    Subject = createDto.Subject,
                    Message = createDto.Message,
                    NotificationType = createDto.NotificationType,
                    TargetRole = createDto.TargetRole,
                    Status = EmailStatus.Pending,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdBroadcast = await _broadcastRepository.AddAsync(broadcast);
                return MapToBroadcastNotificationResponseDto(createdBroadcast);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending broadcast notification: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        
        }

        public async Task<BroadcastNotificationResponseDto> UpdateAsync(int id, UpdateBroadcastNotificationDto updateDto)
        {
            try
            {
                var broadcast = await _broadcastRepository.GetByIdAsync(id);
                if (broadcast == null)
                    throw new ArgumentException($"Broadcast notification with Id {id} not found.");

                broadcast.Subject = updateDto.Subject;
                broadcast.Message = updateDto.Message;
                broadcast.NotificationType = updateDto.NotificationType;
                broadcast.TargetRole = updateDto.TargetRole;
                broadcast.IsActive = updateDto.IsActive;

                var updatedBroadcast = await _broadcastRepository.UpdateAsync(broadcast);
                return MapToBroadcastNotificationResponseDto(updatedBroadcast);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating broadcast notification with Id {id}.", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var exists = await _broadcastRepository.ExistsAsync(id);
                if (!exists)
                    throw new ArgumentException($"Broadcast notification with Id {id} not found.");

                return await _broadcastRepository.DeleteAsync(id);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting broadcast notification with Id {id}.", ex);
            }
        }

        // Broadcast-specific methods
        //public async Task<bool> SendBroadcastAsync(int broadcastId)
        //{
        //    try
        //    {
        //        var broadcast = await _broadcastRepository.GetByIdAsync(broadcastId);
        //        if (broadcast == null)
        //            throw new ArgumentException($"Broadcast notification with Id {broadcastId} not found.");

        //        if (broadcast.Status != EmailStatus.Pending)
        //            throw new InvalidOperationException("Only pending broadcasts can be sent.");

        //        // Get target users
        //        IEnumerable<User> targetUsers;
        //        if (broadcast.TargetRole.HasValue)
        //        {
        //            targetUsers = await _userRepository.GetByRoleAsync(broadcast.TargetRole.Value);
        //        }
        //        else
        //        {
        //            targetUsers = await _userRepository.GetAllAsync();
        //        }

        //        // Filter active users only
        //        targetUsers = targetUsers.Where(u => u.IsActive);

        //        var successCount = 0;
        //        var errorMessages = new List<string>();

        //        foreach (var user in targetUsers)
        //        {
        //            try
        //            {
        //                var personalizedMessage = $"Dear {user.FirstName} {user.LastName},\n\n{broadcast.Message}\n\nBest regards,\nSmart Parking System Team";

        //                var emailDto = new DTOs.EmailNotification.SendEmailNotificationDto
        //                {
        //                    UserId = user.Id,
        //                    ParkingSessionId = 0, // No specific session for broadcasts
        //                    EmailAddress = user.Email,
        //                    Subject = broadcast.Subject,
        //                    Message = personalizedMessage,
        //                    NotificationType = NotificationType.Entry // Generic type for broadcasts
        //                };

        //                await _emailService.SendNotificationAsync(emailDto);
        //                successCount++;
        //            }
        //            catch (Exception ex)
        //            {
        //                errorMessages.Add($"Failed to send to {user.Email}: {ex.Message}");
        //            }
        //        }

        //        // Update broadcast status
        //        if (successCount > 0)
        //        {
        //            broadcast.Status = EmailStatus.Sent;
        //            broadcast.SentAt = DateTime.UtcNow;
        //            if (errorMessages.Any())
        //            {
        //                broadcast.ErrorMessage = string.Join("; ", errorMessages);
        //            }
        //        }
        //        else
        //        {
        //            broadcast.Status = EmailStatus.Failed;
        //            broadcast.ErrorMessage = string.Join("; ", errorMessages);
        //        }

        //        await _broadcastRepository.UpdateAsync(broadcast);
        //        return successCount > 0;
        //    }
        //    catch (ArgumentException)
        //    {
        //        throw;
        //    }
        //    catch (InvalidOperationException)
        //    {
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error sending broadcast notification.", ex);
        //    }
        //}
        public async Task<bool> SendBroadcastAsync(int broadcastId)
        {
            try
            {
                var broadcast = await _broadcastRepository.GetByIdAsync(broadcastId);
                if (broadcast == null)
                    throw new ArgumentException($"Broadcast notification with Id {broadcastId} not found.");

                if (broadcast.Status != EmailStatus.Pending)
                    throw new InvalidOperationException("Only pending broadcasts can be sent.");

                // Get target users
                IEnumerable<User> targetUsers;
                if (broadcast.TargetRole.HasValue)
                {
                    targetUsers = await _userRepository.GetByRoleAsync(broadcast.TargetRole.Value);
                }
                else
                {
                    targetUsers = await _userRepository.GetAllAsync();
                }

                targetUsers = targetUsers.Where(u => u.IsActive);

                var successCount = 0;
                var errorMessages = new List<string>();

                // Load SMTP settings from appsettings.json
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

                foreach (var user in targetUsers)
                {
                    try
                    {
                        var personalizedMessage =
                            $"Dear {user.FirstName} {user.LastName},\n\n{broadcast.Message}\n\nBest regards,\nSmart Parking System Team";

                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(fromEmail, fromName),
                            Subject = broadcast.Subject,
                            Body = personalizedMessage,
                            IsBodyHtml = false
                        };

                        mailMessage.To.Add(user.Email);

                        await smtpClient.SendMailAsync(mailMessage);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errorMessages.Add($"Failed to send to {user.Email}: {ex.Message}");
                    }
                }

                // Update broadcast status
                if (successCount > 0)
                {
                    broadcast.Status = EmailStatus.Sent;
                    broadcast.SentAt = DateTime.UtcNow;
                    if (errorMessages.Any())
                        broadcast.ErrorMessage = string.Join("; ", errorMessages);
                }
                else
                {
                    broadcast.Status = EmailStatus.Failed;
                    broadcast.ErrorMessage = string.Join("; ", errorMessages);
                }

                await _broadcastRepository.UpdateAsync(broadcast);
                return successCount > 0;
            }
            catch (ArgumentException ex)
            {
                throw new Exception($"Broadcast not sent: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception($"Broadcast not sent: {ex.Message}");
            }
            catch (SmtpException ex)
            {
                throw new Exception($"Email sending failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error while sending broadcast: {ex.Message}");
            }

        }

        public async Task<IEnumerable<BroadcastNotificationResponseDto>> GetByTargetRoleAsync(UserRole? targetRole)
        {
            try
            {
                var broadcasts = await _broadcastRepository.GetByTargetRoleAsync(targetRole);
                return broadcasts.Select(MapToBroadcastNotificationResponseDto);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving broadcast notifications for target role {targetRole}.", ex);
            }
        }

        public async Task<bool> ProcessPendingBroadcastsAsync()
        {
            try
            {
                var pendingBroadcasts = await _broadcastRepository.GetPendingBroadcastsAsync();
                var successCount = 0;

                foreach (var broadcast in pendingBroadcasts)
                {
                    var sent = await SendBroadcastAsync(broadcast.Id);
                    if (sent) successCount++;
                }

                return successCount > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error processing pending broadcast notifications.", ex);
            }
        }

        private BroadcastNotificationResponseDto MapToBroadcastNotificationResponseDto(BroadcastNotification broadcast)
        {
            return new BroadcastNotificationResponseDto
            {
                Id = broadcast.Id,
                Subject = broadcast.Subject,
                Message = broadcast.Message,
                NotificationType = broadcast.NotificationType,
                Status = broadcast.Status,
                SentAt = broadcast.SentAt,
                ErrorMessage = broadcast.ErrorMessage,
                TargetRole = broadcast.TargetRole,
                IsActive = broadcast.IsActive,
                CreatedAt = broadcast.CreatedAt
            };
        }
    }
}
