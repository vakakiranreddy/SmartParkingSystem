using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParkingSystem.DTOs.ParkingSession;
using SmartParkingSystem.Interfaces.Services;
using SmartParkingSystem.Models;
using System.Security.Claims;

namespace SmartParkingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ParkingSessionController : ControllerBase
    {
        private readonly IParkingSessionService _parkingSessionService;

        public ParkingSessionController(IParkingSessionService parkingSessionService)
        {
            _parkingSessionService = parkingSessionService;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var session = await _parkingSessionService.GetByIdAsync(id);
                return Ok(new { success = true, data = session });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving the session." });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var sessions = await _parkingSessionService.GetAllAsync();
                return Ok(new { success = true, data = sessions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving sessions." });
            }
        }

        [HttpPost("start")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> StartWalkInSession([FromBody] StartParkingSessionDto startDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Invalid input data." });

            try
            {
                var session = await _parkingSessionService.StartWalkInSessionAsync(startDto);
                return Ok(new { success = true, data = session });
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
                return StatusCode(500, new { success = false, error = "An error occurred while starting the session." });
            }
        }

        [HttpPost("start-guest")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> StartGuestSession([FromBody] StartGuestSessionDto guestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Invalid input data." });

            try
            {
                var session = await _parkingSessionService.StartGuestSessionAsync(guestDto);
                return Ok(new { success = true, data = session });
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
                return StatusCode(500, new { success = false, error = "An error occurred while starting the guest session." });
            }
        }

        [HttpPut("{id}/fee")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> UpdateFee(int id, [FromBody] UpdateParkingFeeDto updateDto)
        {
            if (!ModelState.IsValid || updateDto.SessionId != id)
                return BadRequest(new { success = false, error = "Invalid input data or session ID mismatch." });

            try
            {
                var session = await _parkingSessionService.UpdateAsync(id, updateDto);
                return Ok(new { success = true, data = session });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while updating the session fee." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _parkingSessionService.DeleteAsync(id);
                return Ok(new { success = true, data = new { message = "Session deleted successfully." } });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while deleting the session." });
            }
        }

        [HttpPost("book")]
        public async Task<IActionResult> BookSlot([FromBody] BookSlotDto bookSlotDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Invalid input data." });

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var session = await _parkingSessionService.BookSlotAsync(userId, bookSlotDto);
                return Ok(new { success = true, data = session });
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"ArgumentException in BookSlot: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"UnauthorizedAccessException in BookSlot: {ex.Message}");
                return Unauthorized(new { success = false, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"InvalidOperationException in BookSlot: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Conflict(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in BookSlot: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return StatusCode(500, new { success = false, error = $"An error occurred while booking the slot: {ex.Message}" });
            }
        }

        [HttpPost("activate")]
        public async Task<IActionResult> ActivateReservation([FromBody] ActivateReservationDto activateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Invalid input data." });

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var session = await _parkingSessionService.ActivateReservationAsync(userId, activateDto);
                return Ok(new { success = true, data = session });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while activating the reservation." });
            }
        }

        [HttpGet("user-reservations")]
        public async Task<IActionResult> GetUserReservations()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var reservations = await _parkingSessionService.GetUserReservationsAsync(userId);
                return Ok(new { success = true, data = reservations });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving reservations." });
            }
        }

        [HttpGet("user-sessions")]
        public async Task<IActionResult> GetUserSessions()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var sessions = await _parkingSessionService.GetUserSessionsAsync(userId);
                return Ok(new { success = true, data = sessions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving user sessions." });
            }
        }

        [HttpPost("cancel/{sessionId}")]
        public async Task<IActionResult> CancelReservation(int sessionId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _parkingSessionService.CancelReservationAsync(userId, sessionId);
                return Ok(new { success = true, data = new { message = "Reservation cancelled successfully." } });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while cancelling the reservation." });
            }
        }

        [HttpPost("end")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> EndSession([FromBody] EndParkingSessionDto endDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Invalid input data." });

            try
            {
                var session = await _parkingSessionService.EndSessionAsync(endDto);
                return Ok(new { success = true, data = session });
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
                return StatusCode(500, new { success = false, error = "An error occurred while ending the session." });
            }
        }

        [HttpPost("process-payment/{sessionId}")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> ProcessPayment(int sessionId, [FromBody] PaymentStatus paymentStatus)
        {
            try
            {
                var result = await _parkingSessionService.ProcessPaymentAsync(sessionId, paymentStatus);
                return Ok(new { success = true, data = new { message = "Payment processed successfully." } });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while processing payment." });
            }
        }

        [HttpGet("unpaid")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> GetUnpaidSessions()
        {
            try
            {
                var sessions = await _parkingSessionService.GetUnpaidSessionsAsync();
                return Ok(new { success = true, data = sessions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving unpaid sessions." });
            }
        }

        [HttpGet("active")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> GetAllActiveSessions()
        {
            try
            {
                var sessions = await _parkingSessionService.GetAllActiveSessionsAsync();
                return Ok(new { success = true, data = sessions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving active sessions." });
            }
        }

        [HttpGet("reservations")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> GetAllReservations()
        {
            try
            {
                var sessions = await _parkingSessionService.GetAllReservationsAsync();
                return Ok(new { success = true, data = sessions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving reservations." });
            }
        }

        [HttpPost("cancel-session/{sessionId}")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> CancelSession(int sessionId)
        {
            try
            {
                var result = await _parkingSessionService.CancelSessionAsync(sessionId);
                return Ok(new { success = true, data = new { message = "Session cancelled successfully." } });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while cancelling the session." });
            }
        }

        [HttpGet("calculate-fee/{sessionId}")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> CalculateParkingFee(int sessionId)
        {
            try
            {
                var fee = await _parkingSessionService.CalculateParkingFeeAsync(sessionId);
                return Ok(new { success = true, data = new { fee } });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while calculating the parking fee." });
            }
        }

        [HttpGet("dashboard/active-slots")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> GetTotalActiveSlots()
        {
            try
            {
                var count = await _parkingSessionService.GetTotalActiveSlotsAsync();
                return Ok(new { success = true, data = new { count } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving active slots count." });
            }
        }

        [HttpGet("dashboard/occupied-slots")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> GetTotalOccupiedSlots()
        {
            try
            {
                var count = await _parkingSessionService.GetTotalOccupiedSlotsAsync();
                return Ok(new { success = true, data = new { count } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving occupied slots count." });
            }
        }

        [HttpGet("dashboard/revenue")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTotalRevenue([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            try
            {
                var revenue = await _parkingSessionService.GetTotalRevenueAsync(fromDate, toDate);
                return Ok(new { success = true, data = new { revenue } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving total revenue." });
            }
        }

        [HttpGet("dashboard/active-sessions")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> GetTotalActiveSessionsCount()
        {
            try
            {
                var count = await _parkingSessionService.GetTotalActiveSessionsCountAsync();
                return Ok(new { success = true, data = new { count } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving active sessions count." });
            }
        }

        [HttpGet("dashboard/reservations")]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> GetTotalReservationsCount()
        {
            try
            {
                var count = await _parkingSessionService.GetTotalReservationsCountAsync();
                return Ok(new { success = true, data = new { count } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving reservations count." });
            }
        }
    }
}


//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using SmartParkingSystem.DTOs.ParkingSession;
//using SmartParkingSystem.Interfaces.Services;
//using SmartParkingSystem.Models;
//using System.Security.Claims;

//namespace SmartParkingSystem.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Authorize]
//    public class ParkingSessionController : ControllerBase
//    {
//        private readonly IParkingSessionService _parkingSessionService;

//        public ParkingSessionController(IParkingSessionService parkingSessionService)
//        {
//            _parkingSessionService = parkingSessionService;
//        }

//        [HttpGet("{id}")]
//        [Authorize(Roles = "Admin,Guard")]
//        public async Task<IActionResult> GetById(int id)
//        {
//            try
//            {
//                var session = await _parkingSessionService.GetByIdAsync(id);
//                return Ok(new { success = true, data = session });
//            }
//            catch (ArgumentException ex)
//            {
//                return BadRequest(new { success = false, error = ex.Message });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while retrieving the session." });
//            }
//        }

//        [HttpGet]
//        [Authorize(Roles = "Admin,Guard")]
//        public async Task<IActionResult> GetAll()
//        {
//            try
//            {
//                var sessions = await _parkingSessionService.GetAllAsync();
//                return Ok(new { success = true, data = sessions });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while retrieving sessions." });
//            }
//        }

//        [HttpPost("start")]
//        [Authorize(Roles = "Admin,Guard")]
//        public async Task<IActionResult> StartWalkInSession([FromBody] StartParkingSessionDto startDto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(new { success = false, error = "Invalid input data." });

//            try
//            {
//                var session = await _parkingSessionService.StartWalkInSessionAsync(startDto);
//                return Ok(new { success = true, data = session });
//            }
//            catch (ArgumentException ex)
//            {
//                return BadRequest(new { success = false, error = ex.Message });
//            }
//            catch (InvalidOperationException ex)
//            {
//                return Conflict(new { success = false, error = ex.Message });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while starting the session." });
//            }
//        }

//        [HttpPut("{id}/fee")]
//        [Authorize(Roles = "Admin,Guard")]
//        public async Task<IActionResult> UpdateFee(int id, [FromBody] UpdateParkingFeeDto updateDto)
//        {
//            if (!ModelState.IsValid || updateDto.SessionId != id)
//                return BadRequest(new { success = false, error = "Invalid input data or session ID mismatch." });

//            try
//            {
//                var session = await _parkingSessionService.UpdateAsync(id, updateDto);
//                return Ok(new { success = true, data = session });
//            }
//            catch (ArgumentException ex)
//            {
//                return BadRequest(new { success = false, error = ex.Message });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while updating the session fee." });
//            }
//        }

//        [HttpDelete("{id}")]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> Delete(int id)
//        {
//            try
//            {
//                var result = await _parkingSessionService.DeleteAsync(id);
//                return Ok(new { success = true, data = new { message = "Session deleted successfully." } });
//            }
//            catch (ArgumentException ex)
//            {
//                return BadRequest(new { success = false, error = ex.Message });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while deleting the session." });
//            }
//        }

//        [HttpPost("book")]
//        public async Task<IActionResult> BookSlot([FromBody] BookSlotDto bookSlotDto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(new { success = false, error = "Invalid input data." });

//            try
//            {
//                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
//                var session = await _parkingSessionService.BookSlotAsync(userId, bookSlotDto);
//                return Ok(new { success = true, data = session });
//            }
//            catch (ArgumentException ex)
//            {
//                return BadRequest(new { success = false, error = ex.Message });
//            }
//            catch (UnauthorizedAccessException ex)
//            {
//                return Unauthorized(new { success = false, error = ex.Message });
//            }
//            catch (InvalidOperationException ex)
//            {
//                return Conflict(new { success = false, error = ex.Message });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while booking the slot." });
//            }
//        }

//        [HttpPost("activate")]
//        public async Task<IActionResult> ActivateReservation([FromBody] ActivateReservationDto activateDto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(new { success = false, error = "Invalid input data." });

//            try
//            {
//                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
//                var session = await _parkingSessionService.ActivateReservationAsync(userId, activateDto);
//                return Ok(new { success = true, data = session });
//            }
//            catch (ArgumentException ex)
//            {
//                return BadRequest(new { success = false, error = ex.Message });
//            }
//            catch (UnauthorizedAccessException ex)
//            {
//                return Unauthorized(new { success = false, error = ex.Message });
//            }
//            catch (InvalidOperationException ex)
//            {
//                return Conflict(new { success = false, error = ex.Message });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while activating the reservation." });
//            }
//        }

//        [HttpGet("user-reservations")]
//        public async Task<IActionResult> GetUserReservations()
//        {
//            try
//            {
//                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
//                var reservations = await _parkingSessionService.GetUserReservationsAsync(userId);
//                return Ok(new { success = true, data = reservations });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while retrieving reservations." });
//            }
//        }

//        [HttpGet("user-sessions")]
//        public async Task<IActionResult> GetUserSessions()
//        {
//            try
//            {
//                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
//                var sessions = await _parkingSessionService.GetUserSessionsAsync(userId);
//                return Ok(new { success = true, data = sessions });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while retrieving user sessions." });
//            }
//        }

//        [HttpPost("cancel/{sessionId}")]
//        public async Task<IActionResult> CancelReservation(int sessionId)
//        {
//            try
//            {
//                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
//                var result = await _parkingSessionService.CancelReservationAsync(userId, sessionId);
//                return Ok(new { success = true, data = new { message = "Reservation cancelled successfully." } });
//            }
//            catch (ArgumentException ex)
//            {
//                return BadRequest(new { success = false, error = ex.Message });
//            }
//            catch (UnauthorizedAccessException ex)
//            {
//                return Unauthorized(new { success = false, error = ex.Message });
//            }
//            catch (InvalidOperationException ex)
//            {
//                return Conflict(new { success = false, error = ex.Message });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while cancelling the reservation." });
//            }
//        }

//        [HttpPost("end")]
//        [Authorize(Roles = "Admin,Guard")]
//        public async Task<IActionResult> EndSession([FromBody] EndParkingSessionDto endDto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(new { success = false, error = "Invalid input data." });

//            try
//            {
//                var session = await _parkingSessionService.EndSessionAsync(endDto);
//                return Ok(new { success = true, data = session });
//            }
//            catch (ArgumentException ex)
//            {
//                return BadRequest(new { success = false, error = ex.Message });
//            }
//            catch (InvalidOperationException ex)
//            {
//                return Conflict(new { success = false, error = ex.Message });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while ending the session." });
//            }
//        }

//        [HttpPost("process-payment/{sessionId}")]
//        [Authorize(Roles = "Admin,Guard")]
//        public async Task<IActionResult> ProcessPayment(int sessionId, [FromBody] PaymentStatus paymentStatus)
//        {
//            try
//            {
//                var result = await _parkingSessionService.ProcessPaymentAsync(sessionId, paymentStatus);
//                return Ok(new { success = true, data = new { message = "Payment processed successfully." } });
//            }
//            catch (ArgumentException ex)
//            {
//                return BadRequest(new { success = false, error = ex.Message });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while processing payment." });
//            }
//        }

//        [HttpGet("unpaid")]
//        [Authorize(Roles = "Admin,Guard")]
//        public async Task<IActionResult> GetUnpaidSessions()
//        {
//            try
//            {
//                var sessions = await _parkingSessionService.GetUnpaidSessionsAsync();
//                return Ok(new { success = true, data = sessions });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while retrieving unpaid sessions." });
//            }
//        }

//        [HttpGet("active")]
//        [Authorize(Roles = "Admin,Guard")]
//        public async Task<IActionResult> GetAllActiveSessions()
//        {
//            try
//            {
//                var sessions = await _parkingSessionService.GetAllActiveSessionsAsync();
//                return Ok(new { success = true, data = sessions });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while retrieving active sessions." });
//            }
//        }

//        [HttpGet("reservations")]
//        [Authorize(Roles = "Admin,Guard")]
//        public async Task<IActionResult> GetAllReservations()
//        {
//            try
//            {
//                var sessions = await _parkingSessionService.GetAllReservationsAsync();
//                return Ok(new { success = true, data = sessions });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while retrieving reservations." });
//            }
//        }

//        [HttpPost("cancel-session/{sessionId}")]
//        [Authorize(Roles = "Admin,Guard")]
//        public async Task<IActionResult> CancelSession(int sessionId)
//        {
//            try
//            {
//                var result = await _parkingSessionService.CancelSessionAsync(sessionId);
//                return Ok(new { success = true, data = new { message = "Session cancelled successfully." } });
//            }
//            catch (ArgumentException ex)
//            {
//                return BadRequest(new { success = false, error = ex.Message });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while cancelling the session." });
//            }
//        }

//        [HttpGet("calculate-fee/{sessionId}")]
//        [Authorize(Roles = "Admin,Guard")]
//        public async Task<IActionResult> CalculateParkingFee(int sessionId)
//        {
//            try
//            {
//                var fee = await _parkingSessionService.CalculateParkingFeeAsync(sessionId);
//                return Ok(new { success = true, data = new { fee } });
//            }
//            catch (ArgumentException ex)
//            {
//                return BadRequest(new { success = false, error = ex.Message });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while calculating the parking fee." });
//            }
//        }

//        [HttpGet("dashboard/active-slots")]
//        [Authorize(Roles = "Admin,Guard")]
//        public async Task<IActionResult> GetTotalActiveSlots()
//        {
//            try
//            {
//                var count = await _parkingSessionService.GetTotalActiveSlotsAsync();
//                return Ok(new { success = true, data = new { count } });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while retrieving active slots count." });
//            }
//        }

//        [HttpGet("dashboard/occupied-slots")]
//        [Authorize(Roles = "Admin,Guard")]
//        public async Task<IActionResult> GetTotalOccupiedSlots()
//        {
//            try
//            {
//                var count = await _parkingSessionService.GetTotalOccupiedSlotsAsync();
//                return Ok(new { success = true, data = new { count } });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while retrieving occupied slots count." });
//            }
//        }

//        [HttpGet("dashboard/revenue")]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> GetTotalRevenue([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
//        {
//            try
//            {
//                var revenue = await _parkingSessionService.GetTotalRevenueAsync(fromDate, toDate);
//                return Ok(new { success = true, data = new { revenue } });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while retrieving total revenue." });
//            }
//        }

//        [HttpGet("dashboard/active-sessions")]
//        [Authorize(Roles = "Admin,Guard")]
//        public async Task<IActionResult> GetTotalActiveSessionsCount()
//        {
//            try
//            {
//                var count = await _parkingSessionService.GetTotalActiveSessionsCountAsync();
//                return Ok(new { success = true, data = new { count } });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while retrieving active sessions count." });
//            }
//        }

//        [HttpGet("dashboard/reservations")]
//        [Authorize(Roles = "Admin,Guard")]
//        public async Task<IActionResult> GetTotalReservationsCount()
//        {
//            try
//            {
//                var count = await _parkingSessionService.GetTotalReservationsCountAsync();
//                return Ok(new { success = true, data = new { count } });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "An error occurred while retrieving reservations count." });
//            }
//        }
//    }
//}
