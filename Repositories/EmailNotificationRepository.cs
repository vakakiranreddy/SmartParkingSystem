using Microsoft.EntityFrameworkCore;
using SmartParkingSystem.Data;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Repositories
{
    public class EmailNotificationRepository : BaseRepository<EmailNotification>, IEmailNotificationRepository
    {
        private readonly ParkingDbContext _context;

        public EmailNotificationRepository(ParkingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmailNotification>> GetByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Set<EmailNotification>()
                    .Where(e => e.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching email notifications for user Id {userId}.", ex);
            }
        }

        public async Task<IEnumerable<EmailNotification>> GetPendingNotificationsAsync()
        {
            try
            {
                return await _context.Set<EmailNotification>()
                    .Where(e => e.Status == EmailStatus.Pending)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching pending email notifications.", ex);
            }
        }

        public async Task<IEnumerable<EmailNotification>> GetByStatusAsync(EmailStatus status)
        {
            try
            {
                return await _context.Set<EmailNotification>()
                    .Where(e => e.Status == status)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching email notifications with status {status}.", ex);
            }
        }
    }
}
