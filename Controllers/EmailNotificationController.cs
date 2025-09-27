using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParkingSystem.DTOs.EmailNotification;
using SmartParkingSystem.Interfaces.Services;

namespace SmartParkingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmailNotificationController : ControllerBase
    {
        private readonly IEmailNotificationService _emailService;

        public EmailNotificationController(IEmailNotificationService emailService)
        {
            _emailService = emailService;
        }

        /// <summary>
        /// Send email notification to a user
        /// </summary>
        [HttpPost("send")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<ActionResult<EmailNotificationResponseDto>> SendNotification([FromBody] SendEmailNotificationDto notificationDto)
        {
            try
            {
                var result = await _emailService.SendNotificationAsync(notificationDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while sending the notification.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all notifications for a specific user
        /// </summary>
        //[HttpGet("user/{userId}")]
        //public async Task<ActionResult<IEnumerable<EmailNotificationResponseDto>>> GetUserNotifications(int userId)
        //{
        //    try
        //    {
        //        // Users can only view their own notifications, unless they're admin/guard
        //        var currentUserRole = User.FindFirst("role")?.Value;
        //        var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");

        //        if (currentUserRole != "Admin" && currentUserRole != "Guard" && currentUserId != userId)
        //        {
        //            return Forbid("You can only view your own notifications.");
        //        }

        //        var notifications = await _emailService.GetUserNotificationsAsync(userId);
        //        return Ok(notifications);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while retrieving notifications.", details = ex.Message });
        //    }
        //}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<EmailNotificationResponseDto>>> GetUserNotifications(int userId)
        {
            try
            {
                // Users can only view their own notifications, unless they're admin/guard
                var currentUserRole = User.FindFirst("role")?.Value;
                var currentUserId = int.Parse(User.FindFirst("nameid")?.Value ?? "0"); // <-- FIXED

                if (currentUserRole != "Admin" && currentUserRole != "Guard" && currentUserId != userId)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new { message = "You can only view your own notifications." });
                }

                var notifications = await _emailService.GetUserNotificationsAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving notifications.", details = ex.Message });
            }
        }


        /// <summary>
        /// Process all pending emails (Admin/Guard only)
        /// </summary>
        [HttpPost("process-pending")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<ActionResult<bool>> ProcessPendingEmails()
        {
            try
            {
                var result = await _emailService.ProcessPendingEmailsAsync();
                return Ok(new { processed = result, message = result ? "Pending emails processed successfully." : "No pending emails to process." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing pending emails.", details = ex.Message });
            }
        }
    }
}
