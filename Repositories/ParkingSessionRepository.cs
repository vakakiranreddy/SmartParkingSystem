using Microsoft.EntityFrameworkCore;
using SmartParkingSystem.Data;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Repositories
{
    public class ParkingSessionRepository : BaseRepository<ParkingSession>, IParkingSessionRepository
    {
        private readonly ParkingDbContext _context;

        public ParkingSessionRepository(ParkingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ParkingSession>> GetByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Set<ParkingSession>()
                    .Where(s => s.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching sessions for user Id {userId}.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSession>> GetActiveSessionsAsync()
        {
            try
            {
                return await _context.Set<ParkingSession>()
                    .Where(s => s.Status == SessionStatus.Active)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching active sessions.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSession>> GetReservationsAsync()
        {
            try
            {
                return await _context.Set<ParkingSession>()
                    .Where(s => s.Status == SessionStatus.Reserved)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching reservations.", ex);
            }
        }

        public async Task<ParkingSession> GetActiveSessionBySlotIdAsync(int slotId)
        {
            try
            {
                return await _context.Set<ParkingSession>()
                    .FirstOrDefaultAsync(s => s.SlotId == slotId && s.Status == SessionStatus.Active);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching active session for slot Id {slotId}.", ex);
            }
        }

        public async Task<ParkingSession> GetSessionWithDetailsAsync(int sessionId)
        {
            try
            {
                return await _context.Set<ParkingSession>()
                    .Include(s => s.User)
                    .Include(s => s.Vehicle)
                    .Include(s => s.ParkingSlot)
                    .FirstOrDefaultAsync(s => s.Id == sessionId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching session details for Id {sessionId}.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSession>> GetUserReservationsAsync(int userId)
        {
            try
            {
                return await _context.Set<ParkingSession>()
                    .Where(s => s.UserId == userId && s.Status == SessionStatus.Reserved)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching reservations for user Id {userId}.", ex);
            }
        }

        public async Task<bool> HasActiveSessionAsync(int vehicleId)
        {
            try
            {
                return await _context.Set<ParkingSession>()
                    .AnyAsync(s => s.VehicleId == vehicleId && s.Status == SessionStatus.Active);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking active session for vehicle Id {vehicleId}.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSession>> GetUnpaidSessionsAsync()
        {
            try
            {
                return await _context.Set<ParkingSession>()
                    .Where(s => s.PaymentStatus == PaymentStatus.Pending)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching unpaid sessions.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSession>> GetSessionsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                return await _context.Set<ParkingSession>()
                    .Where(s => s.EntryTime >= fromDate && s.EntryTime <= toDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching sessions in date range.", ex);
            }
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                return await _context.Set<ParkingSession>()
                    .Where(s => s.EntryTime >= fromDate && s.EntryTime <= toDate &&
                                s.PaymentStatus == PaymentStatus.Paid)
                    .SumAsync(s => s.ParkingFee);
            }
            catch (Exception ex)
            {
                throw new Exception("Error calculating total revenue.", ex);
            }
        }

        public async Task<int> GetActiveSessionsCountAsync()
        {
            try
            {
                return await _context.Set<ParkingSession>()
                    .CountAsync(s => s.Status == SessionStatus.Active);
            }
            catch (Exception ex)
            {
                throw new Exception("Error counting active sessions.", ex);
            }
        }

        public async Task<int> GetOccupiedSlotsCountAsync()
        {
            try
            {
                return await _context.Set<ParkingSession>()
                    .Where(s => s.Status == SessionStatus.Active)
                    .Select(s => s.SlotId)
                    .Distinct()
                    .CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error counting occupied slots.", ex);
            }
        }
    }
}
