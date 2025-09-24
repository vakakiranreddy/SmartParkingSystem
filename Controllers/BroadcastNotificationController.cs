using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParkingSystem.DTOs.BroadcastNotification;
using SmartParkingSystem.Interfaces.Services;
using SmartParkingSystem.Models;
using System.Net.Mail;

namespace SmartParkingSystem.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class BroadcastNotificationController : ControllerBase
    {
        private readonly IBroadcastNotificationService _broadcastService;

        public BroadcastNotificationController(IBroadcastNotificationService broadcastService)
        {
            _broadcastService = broadcastService;
        }

        /// <summary>
        /// Get all broadcast notifications
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BroadcastNotificationResponseDto>>> GetAllBroadcasts()
        {
            try
            {
                var broadcasts = await _broadcastService.GetAllAsync();
                return Ok(broadcasts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving broadcasts.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get broadcast notification by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BroadcastNotificationResponseDto>> GetBroadcastById(int id)
        {
            try
            {
                var broadcast = await _broadcastService.GetByIdAsync(id);
                return Ok(broadcast);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the broadcast.", details = ex.Message });
            }
        }

        /// <summary>
        /// Create new broadcast notification
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BroadcastNotificationResponseDto>> CreateBroadcast([FromBody] CreateBroadcastNotificationDto createDto)
        {
            try
            {
                var broadcast = await _broadcastService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetBroadcastById), new { id = broadcast.Id }, broadcast);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the broadcast.", details = ex.Message });
            }
        }

        /// <summary>
        /// Update existing broadcast notification
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<BroadcastNotificationResponseDto>> UpdateBroadcast(int id, [FromBody] UpdateBroadcastNotificationDto updateDto)
        {
            try
            {
                var broadcast = await _broadcastService.UpdateAsync(id, updateDto);
                return Ok(broadcast);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the broadcast.", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete broadcast notification
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBroadcast(int id)
        {
            try
            {
                var deleted = await _broadcastService.DeleteAsync(id);
                return Ok(new { message = "Broadcast deleted successfully." });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the broadcast.", details = ex.Message });
            }
        }

        /// <summary>
        /// Send broadcast notification to target users
        /// </summary>
        [HttpPost("{id}/send")]
        public async Task<ActionResult> SendBroadcast(int id)
        {
            try
            {
                var result = await _broadcastService.SendBroadcastAsync(id);
                if (result)
                {
                    return Ok(new { message = "Broadcast sent successfully." });
                }
                else
                {
                    return BadRequest(new { message = "Failed to send broadcast. No users were notified." });
                }
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (SmtpException ex)
            {
                return StatusCode(500, new { success = false, message = $"Email sending failed: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }
        
        

        /// <summary>
        /// Get broadcasts by target role
        /// </summary>
        [HttpGet("role/{role}")]
        public async Task<ActionResult<IEnumerable<BroadcastNotificationResponseDto>>> GetBroadcastsByRole(UserRole role)
        {
            try
            {
                var broadcasts = await _broadcastService.GetByTargetRoleAsync(role);
                return Ok(broadcasts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving broadcasts.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all broadcasts targeting all users
        /// </summary>
        [HttpGet("all-users")]
        public async Task<ActionResult<IEnumerable<BroadcastNotificationResponseDto>>> GetBroadcastsForAllUsers()
        {
            try
            {
                var broadcasts = await _broadcastService.GetByTargetRoleAsync(null);
                return Ok(broadcasts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving broadcasts.", details = ex.Message });
            }
        }

        /// <summary>
        /// Process all pending broadcasts
        /// </summary>
        [HttpPost("process-pending")]
        public async Task<ActionResult> ProcessPendingBroadcasts()
        {
            try
            {
                var result = await _broadcastService.ProcessPendingBroadcastsAsync();
                return Ok(new { processed = result, message = result ? "Pending broadcasts processed successfully." : "No pending broadcasts to process." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing pending broadcasts.", details = ex.Message });
            }
        }
    }
}
