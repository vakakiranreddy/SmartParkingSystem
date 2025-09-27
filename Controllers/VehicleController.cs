using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParkingSystem.DTOs.Vehicle;
using SmartParkingSystem.Interfaces.Services;
using SmartParkingSystem.Models;
using System.Security.Claims;

namespace SmartParkingSystem.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var vehicle = await _vehicleService.GetByIdAsync(id);
                return Ok(new { success = true, data = vehicle });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving the vehicle." });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var vehicles = await _vehicleService.GetAllAsync();
                return Ok(new { success = true, data = vehicles });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving vehicles." });
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] CreateVehicleDto createDto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(new { success = false, error = "Invalid input data." });

        //    try
        //    {
        //        if (!Enum.IsDefined(typeof(VehicleType), createDto.VehicleType))
        //            return BadRequest(new { success = false, error = "Invalid vehicle type." });

        //        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        //        // Default: current user is owner
        //        var ownerId = userId;

        //        // If Admin/Guard, allow overriding (optional)
        //        if (userRole == "Admin" || userRole == "Guard")
        //        {
        //            // you could allow them to assign OwnerId if needed
        //            ownerId = userId; // keep as self for now
        //        }

        //        var vehicle = await _vehicleService.CreateAsync(createDto, ownerId);
        //        return Ok(new { success = true, data = vehicle });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return Conflict(new { success = false, error = ex.Message });
        //    }
        //    catch
        //    {
        //        return StatusCode(500, new { success = false, error = "An error occurred while creating the vehicle." });
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateVehicleDto createDto, IFormFile vehicleImage)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Invalid input data." });

            try
            {
                if (!Enum.IsDefined(typeof(VehicleType), createDto.VehicleType))
                    return BadRequest(new { success = false, error = "Invalid vehicle type." });

                byte[] imageBytes = null;
                if (vehicleImage != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await vehicleImage.CopyToAsync(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var ownerId = userId;

                if (userRole == "Admin" || userRole == "Guard")
                {
                    ownerId = userId;
                }

                var vehicle = await _vehicleService.CreateAsync(createDto, ownerId, imageBytes);
                return Ok(new { success = true, data = vehicle });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, error = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "An error occurred while creating the vehicle." });
            }
        }


        //[HttpPut("{id}")]
        //public async Task<IActionResult> Update(int id, [FromBody] UpdateVehicleDto updateDto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(new { success = false, error = "Invalid input data." });

        //    try
        //    {
        //        // Validate VehicleType
        //        if (!Enum.IsDefined(typeof(VehicleType), updateDto.VehicleType))
        //            return BadRequest(new { success = false, error = "Invalid vehicle type." });

        //        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        //        var vehicle = await _vehicleService.GetByIdAsync(id);

        //        // Only Admin and Guard can update any vehicle; User can only update their own
        //        if (userRole != "Admin" && userRole != "Guard" && vehicle.OwnerId != userId)
        //        {
        //            return Unauthorized(new { success = false, error = "You are not authorized to update this vehicle." });
        //        }

        //        var updatedVehicle = await _vehicleService.UpdateAsync(id, updateDto);
        //        return Ok(new { success = true, data = updatedVehicle });
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new { success = false, error = ex.Message });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return Conflict(new { success = false, error = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { success = false, error = "An error occurred while updating the vehicle." });
        //    }
        //}

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateVehicleDto updateDto, IFormFile vehicleImage)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Invalid input data." });

            try
            {
                if (!Enum.IsDefined(typeof(VehicleType), updateDto.VehicleType))
                    return BadRequest(new { success = false, error = "Invalid vehicle type." });

                byte[] imageBytes = null;
                if (vehicleImage != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await vehicleImage.CopyToAsync(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var vehicle = await _vehicleService.GetByIdAsync(id);

                if (userRole != "Admin" && userRole != "Guard" && vehicle.OwnerId != userId)
                {
                    return Unauthorized(new { success = false, error = "You are not authorized to update this vehicle." });
                }

                var updatedVehicle = await _vehicleService.UpdateAsync(id, updateDto, imageBytes);
                return Ok(new { success = true, data = updatedVehicle });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while updating the vehicle." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _vehicleService.DeleteAsync(id);
                return Ok(new { success = true, data = new { message = "Vehicle deleted successfully." } });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while deleting the vehicle." });
            }
        }

        [HttpGet("user-vehicles")]
        public async Task<IActionResult> GetUserVehicles()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var vehicles = await _vehicleService.GetUserVehiclesAsync(userId);
                return Ok(new { success = true, data = vehicles });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving user vehicles." });
            }
        }

        [HttpGet("by-license-plate/{licensePlate}")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> GetByLicensePlate(string licensePlate)
        {
            try
            {
                var vehicle = await _vehicleService.GetByLicensePlateAsync(licensePlate);
                return Ok(new { success = true, data = vehicle });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving the vehicle." });
            }
        }
    }
}
