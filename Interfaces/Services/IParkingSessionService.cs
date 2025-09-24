using SmartParkingSystem.DTOs.ParkingSession;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Services
{
    public interface IParkingSessionService
    {
        // Basic CRUD
        Task<ParkingSessionResponseDto> GetByIdAsync(int id);
        Task<IEnumerable<ParkingSessionResponseDto>> GetAllAsync();
        Task<ParkingSessionResponseDto> CreateAsync(StartParkingSessionDto createDto);
        Task<ParkingSessionResponseDto> UpdateAsync(int id, UpdateParkingFeeDto updateDto);
        Task<bool> DeleteAsync(int id);

        // User operations
        Task<ParkingSessionResponseDto> BookSlotAsync(int userId, BookSlotDto bookSlotDto);
        Task<ParkingSessionResponseDto> ActivateReservationAsync(int userId, ActivateReservationDto activateDto);
        Task<IEnumerable<ReservationDto>> GetUserReservationsAsync(int userId);
        Task<IEnumerable<ParkingSessionResponseDto>> GetUserSessionsAsync(int userId);
        Task<bool> CancelReservationAsync(int userId, int sessionId);

        // Guard operations
        Task<ParkingSessionResponseDto> StartWalkInSessionAsync(StartParkingSessionDto startSessionDto);
        Task<ParkingSessionResponseDto> EndSessionAsync(EndParkingSessionDto endSessionDto);

        // Payment operations
        Task<bool> ProcessPaymentAsync(int sessionId, PaymentStatus paymentStatus);
        Task<IEnumerable<ParkingSessionResponseDto>> GetUnpaidSessionsAsync();

        // Admin operations
        Task<IEnumerable<ParkingSessionResponseDto>> GetAllActiveSessionsAsync();
        Task<IEnumerable<ParkingSessionResponseDto>> GetAllReservationsAsync();
        Task<bool> CancelSessionAsync(int sessionId);
        Task<decimal> CalculateParkingFeeAsync(int sessionId);

        // Dashboard/Reports
        Task<int> GetTotalActiveSlotsAsync();
        Task<int> GetTotalOccupiedSlotsAsync();
        Task<decimal> GetTotalRevenueAsync(DateTime fromDate, DateTime toDate);
        Task<int> GetTotalActiveSessionsCountAsync();
        Task<int> GetTotalReservationsCountAsync();
    }
}
