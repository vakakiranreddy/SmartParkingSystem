using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
        Task<User> GetUserWithVehiclesAsync(int userId);
        Task<bool> EmailExistsAsync(string email);
    }
}
