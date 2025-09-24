using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Repositories
{
    public interface IParkingSessionRepository : IBaseRepository<ParkingSession>
    {
        Task<IEnumerable<ParkingSession>> GetByUserIdAsync(int userId);
        Task<IEnumerable<ParkingSession>> GetActiveSessionsAsync();
        Task<IEnumerable<ParkingSession>> GetReservationsAsync();
        Task<ParkingSession> GetActiveSessionBySlotIdAsync(int slotId);
        Task<ParkingSession> GetSessionWithDetailsAsync(int sessionId);
        Task<IEnumerable<ParkingSession>> GetUserReservationsAsync(int userId);
        Task<bool> HasActiveSessionAsync(int vehicleId);
        Task<IEnumerable<ParkingSession>> GetUnpaidSessionsAsync();
        Task<IEnumerable<ParkingSession>> GetSessionsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<decimal> GetTotalRevenueAsync(DateTime fromDate, DateTime toDate);
        Task<int> GetActiveSessionsCountAsync();
        Task<int> GetOccupiedSlotsCountAsync();
    }
}
