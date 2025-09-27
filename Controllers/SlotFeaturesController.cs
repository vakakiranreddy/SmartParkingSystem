using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParkingSystem.DTOs.SlotFeature;
using SmartParkingSystem.Interfaces.Services;

namespace SmartParkingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SlotFeaturesController : ControllerBase
    {
        private readonly ISlotFeatureService _slotFeatureService;

        public SlotFeaturesController(ISlotFeatureService slotFeatureService)
        {
            _slotFeatureService = slotFeatureService;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignFeature([FromBody] AssignSlotFeatureDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Invalid input." });

            try
            {
                var result = await _slotFeatureService.AssignFeatureToSlotAsync(dto);
                return Ok(new { success = true, data = result });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to assign feature." });
            }
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveFeature([FromBody] RemoveSlotFeatureDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Invalid input." });

            try
            {
                var removed = await _slotFeatureService.RemoveFeatureFromSlotAsync(dto);
                return Ok(new { success = true, data = new { removed } });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to remove feature." });
            }
        }

        [HttpGet("slot/{slotId:int}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetSlotFeatures(int slotId)
        {
            try
            {
                var features = await _slotFeatureService.GetSlotFeaturesAsync(slotId);
                return Ok(new { success = true, data = features });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to retrieve slot features." });
            }
        }

        [HttpGet("feature/{featureId:int}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetFeatureAssignments(int featureId)
        {
            try
            {
                var slots = await _slotFeatureService.GetFeatureAssignmentsAsync(featureId);
                return Ok(new { success = true, data = slots });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to retrieve feature assignments." });
            }
        }
    }
}
