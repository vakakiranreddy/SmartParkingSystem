using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParkingSystem.DTOs.ParkingSlot;
using SmartParkingSystem.Interfaces.Services;

namespace SmartParkingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ParkingSlotsController : ControllerBase
    {
        private readonly IParkingSlotService _parkingSlotService;

        public ParkingSlotsController(IParkingSlotService parkingSlotService)
        {
            _parkingSlotService = parkingSlotService;
        }

      

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var slots = await _parkingSlotService.GetAllAsync();
                return Ok(new { success = true, data = slots });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to retrieve parking slots." });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var slot = await _parkingSlotService.GetByIdAsync(id);
                return Ok(new { success = true, data = slot });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to retrieve parking slot." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateParkingSlotDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Invalid input data." });

            try
            {
                var created = await _parkingSlotService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { success = true, data = created });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, error = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to create parking slot." });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateParkingSlotDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Invalid input data." });

            try
            {
                var updated = await _parkingSlotService.UpdateAsync(id, dto);
                return Ok(new { success = true, data = updated });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, error = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to update parking slot." });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _parkingSlotService.DeleteAsync(id);
                return deleted
                    ? Ok(new { success = true, data = new { message = "Parking slot deleted successfully." } })
                    : NotFound(new { success = false, error = "Parking slot not found." });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to delete parking slot." });
            }
        }

        // ---------- SLOT QUERIES ----------

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable()
        {
            try
            {
                var slots = await _parkingSlotService.GetAvailableSlotsAsync();
                return Ok(new { success = true, data = slots });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to retrieve available slots." });
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SlotSearchDto searchDto)
        {
            try
            {
                var slots = await _parkingSlotService.SearchSlotsAsync(searchDto);
                return Ok(new { success = true, data = slots });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to search slots." });
            }
        }

        [HttpGet("floor/{floor}")]
        public async Task<IActionResult> GetByFloor(string floor)
        {
            try
            {
                var slots = await _parkingSlotService.GetSlotsByFloorAsync(floor);
                return Ok(new { success = true, data = slots });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to retrieve slots by floor." });
            }
        }

        [HttpGet("{id:int}/features")]
        public async Task<IActionResult> GetSlotWithFeatures(int id)
        {
            try
            {
                var slot = await _parkingSlotService.GetSlotWithFeaturesAsync(id);
                return Ok(new { success = true, data = slot });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to retrieve slot with features." });
            }
        }

        [HttpGet("feature/{featureId:int}")]
        public async Task<IActionResult> GetByFeature(int featureId)
        {
            try
            {
                var slots = await _parkingSlotService.GetSlotsByFeatureAsync(featureId);
                return Ok(new { success = true, data = slots });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to retrieve slots by feature." });
            }
        }

        [HttpPost("features")]
        public async Task<IActionResult> GetByFeatures([FromBody] List<int> featureIds)
        {
            try
            {
                var slots = await _parkingSlotService.GetSlotsWithFeaturesAsync(featureIds);
                return Ok(new { success = true, data = slots });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to retrieve slots with features." });
            }
        }

        [HttpPost("features/available")]
        public async Task<IActionResult> GetAvailableByFeatures([FromBody] List<int> featureIds)
        {
            try
            {
                var slots = await _parkingSlotService.GetAvailableSlotsWithFeaturesAsync(featureIds);
                return Ok(new { success = true, data = slots });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to retrieve available slots with features." });
            }
        }

        // ---------- BULK OPERATIONS ----------

        [HttpPut("bulk/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkStatusUpdateRequest request)
        {
            try
            {
                var result = await _parkingSlotService.BulkUpdateSlotStatusAsync(request.SlotIds, request.IsActive);
                return Ok(new { success = result });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to update slot status." });
            }
        }

        [HttpPut("bulk/occupancy")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> BulkUpdateOccupancy([FromBody] BulkOccupancyUpdateRequest request)
        {
            try
            {
                var result = await _parkingSlotService.BulkUpdateSlotOccupancyAsync(request.SlotIds, request.IsOccupied);
                return Ok(new { success = result });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to update slot occupancy." });
            }
        }
    }

    // ---------- Request DTOs ----------
    public class BulkStatusUpdateRequest
    {
        public List<int> SlotIds { get; set; } = new();
        public bool IsActive { get; set; }
    }

    public class BulkOccupancyUpdateRequest
    {
        public List<int> SlotIds { get; set; } = new();
        public bool IsOccupied { get; set; }
    }
}
