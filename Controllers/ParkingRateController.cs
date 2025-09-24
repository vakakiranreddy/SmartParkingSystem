using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParkingSystem.DTOs.ParkingRate;
using SmartParkingSystem.Interfaces.Services;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ParkingRateController : ControllerBase
    {
        private readonly IParkingRateService _parkingRateService;

        public ParkingRateController(IParkingRateService parkingRateService)
        {
            _parkingRateService = parkingRateService;
        }

        /// <summary>
        /// Get all parking rates
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ParkingRateResponseDto>>> GetAllRates()
        {
            try
            {
                var rates = await _parkingRateService.GetAllAsync();
                return Ok(rates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving parking rates.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get parking rate by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ParkingRateResponseDto>> GetRateById(int id)
        {
            try
            {
                var rate = await _parkingRateService.GetByIdAsync(id);
                return Ok(rate);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the parking rate.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get parking rate by vehicle type
        /// </summary>
        [HttpGet("vehicle-type/{vehicleType}")]
        public async Task<ActionResult<ParkingRateResponseDto>> GetRateByVehicleType(VehicleType vehicleType)
        {
            try
            {
                var rate = await _parkingRateService.GetRateByVehicleTypeAsync(vehicleType);
                return Ok(rate);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the parking rate.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all active parking rates
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<ParkingRateResponseDto>>> GetActiveRates()
        {
            try
            {
                var rates = await _parkingRateService.GetActiveRatesAsync();
                return Ok(rates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active parking rates.", details = ex.Message });
            }
        }

        /// <summary>
        /// Create new parking rate (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ParkingRateResponseDto>> CreateRate([FromBody] CreateParkingRateDto createDto)
        {
            try
            {
                var rate = await _parkingRateService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetRateById), new { id = rate.Id }, rate);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the parking rate.", details = ex.Message });
            }
        }

        /// <summary>
        /// Update parking rate (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ParkingRateResponseDto>> UpdateRate(int id, [FromBody] UpdateParkingRateDto updateDto)
        {
            try
            {
                var rate = await _parkingRateService.UpdateAsync(id, updateDto);
                return Ok(rate);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the parking rate.", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete parking rate (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteRate(int id)
        {
            try
            {
                var deleted = await _parkingRateService.DeleteAsync(id);
                return Ok(new { message = "Parking rate deleted successfully." });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the parking rate.", details = ex.Message });
            }
        }
    }
}
