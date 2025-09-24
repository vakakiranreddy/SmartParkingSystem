using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParkingSystem.DTOs.User;
using SmartParkingSystem.Interfaces.Services;
using SmartParkingSystem.Models;
using System.Security.Claims;
using static SmartParkingSystem.Interfaces.Services.IUserService;

namespace SmartParkingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllAsync();
                return Ok(new { success = true, users = users });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Failed to get users" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                return Ok(new { success = true, user = user });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Failed to get user" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDto createDto)
        {
            try
            {
                var user = await _userService.CreateAsync(createDto);
                return Ok(new { success = true, user = user });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Failed to create user" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateDto)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Only allow users to update their own profile or admins to update any
                if (currentUserId != id && userRole != "Admin")
                {
                    return Forbid();
                }

                var user = await _userService.UpdateAsync(id, updateDto);
                return Ok(new { success = true, user = user });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Failed to update user" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userService.DeleteAsync(id);
                return Ok(new { success = true, message = "User deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Failed to delete user" });
            }
        }

        [HttpGet("by-role/{role}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersByRole(UserRole role)
        {
            try
            {
                var users = await _userService.GetByRoleAsync(role);
                return Ok(new { success = true, users = users });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Failed to get users by role" });
            }
        }

        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto roleDto)
        {
            try
            {
                roleDto.UserId = id;
                var result = await _userService.UpdateUserRoleAsync(roleDto);
                return Ok(new { success = true, message = "User role updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Failed to update user role" });
            }
        }

        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            try
            {
                var result = await _userService.DeactivateUserAsync(id);
                return Ok(new { success = true, message = "User deactivated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Failed to deactivate user" });
            }
        }

        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            try
            {
                var result = await _userService.ActivateUserAsync(id);
                return Ok(new { success = true, message = "User activated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Failed to activate user" });
            }
        }

        [HttpGet("{id}/with-vehicles")]
        public async Task<IActionResult> GetUserWithVehicles(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Only allow users to see their own vehicles or admins/guards to see any
                if (currentUserId != id && userRole != "Admin" && userRole != "Guard")
                {
                    return Forbid();
                }

                var user = await _userService.GetUserWithVehiclesAsync(id);
                return Ok(new { success = true, user = user });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Failed to get user with vehicles" });
            }
        }
    }
}
