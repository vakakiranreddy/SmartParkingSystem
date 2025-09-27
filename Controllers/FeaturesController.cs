using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SmartParkingSystem.Interfaces.Services;

namespace SmartParkingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FeaturesController : ControllerBase
    {
        private readonly IFeatureService _featureService;

        public FeaturesController(IFeatureService featureService)
        {
            _featureService = featureService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var features = await _featureService.GetAllAsync();
                return Ok(new { success = true, data = features });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to retrieve features." });
            }
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var feature = await _featureService.GetByIdAsync(id);
                return Ok(new { success = true, data = feature });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to retrieve feature." });
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] CreateFeatureDto dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(new { success = false, error = "Invalid input." });

        //    try
        //    {
        //        var created = await _featureService.CreateAsync(dto);
        //        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { success = true, data = created });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return Conflict(new { success = false, error = ex.Message });
        //    }
        //    catch
        //    {
        //        return StatusCode(500, new { success = false, error = "Failed to create feature." });
        //    }
        //}

        //[HttpPut("{id:int}")]
        //public async Task<IActionResult> Update(int id, [FromBody] UpdateFeatureDto dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(new { success = false, error = "Invalid input." });

        //    try
        //    {
        //        var updated = await _featureService.UpdateAsync(id, dto);
        //        return Ok(new { success = true, data = updated });
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return NotFound(new { success = false, error = ex.Message });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return Conflict(new { success = false, error = ex.Message });
        //    }
        //    catch
        //    {
        //        return StatusCode(500, new { success = false, error = "Failed to update feature." });
        //    }
        //}
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateFeatureDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Invalid input." });

            try
            {
                var created = await _featureService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { success = true, data = created });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, error = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to create feature." });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateFeatureDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Invalid input." });

            try
            {
                var updated = await _featureService.UpdateAsync(id, dto);
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
                return StatusCode(500, new { success = false, error = "Failed to update feature." });
            }
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _featureService.DeleteAsync(id);
                return deleted
                    ? Ok(new { success = true, data = new { message = "Feature deleted successfully." } })
                    : NotFound(new { success = false, error = "Feature not found." });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to delete feature." });
            }
        }

        [HttpGet("active")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                var active = await _featureService.GetActiveFeaturesAsync();
                return Ok(new { success = true, data = active });
            }
            catch
            {
                return StatusCode(500, new { success = false, error = "Failed to retrieve active features." });
            }
        }
    }
}
