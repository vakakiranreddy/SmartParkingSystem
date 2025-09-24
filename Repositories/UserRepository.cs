using Microsoft.EntityFrameworkCore;
using SmartParkingSystem.Data;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly ParkingDbContext _context;

        public UserRepository(ParkingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            try
            {
                return await _context.Set<User>()
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching user with email {email}.", ex);
            }
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
        {
            try
            {
                return await _context.Set<User>()
                    .Where(u => u.Role == role)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching users with role {role}.", ex);
            }
        }

        public async Task<User> GetUserWithVehiclesAsync(int userId)
        {
            try
            {
                return await _context.Set<User>()
                    .Include(u => u.Vehicles)
                    .FirstOrDefaultAsync(u => u.Id == userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching user with vehicles for Id {userId}.", ex);
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                return await _context.Set<User>()
                    .AnyAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if email {email} exists.", ex);
            }
        }
    }
}
