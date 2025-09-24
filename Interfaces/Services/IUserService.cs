using SmartParkingSystem.DTOs.User;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Services
{
   
        public interface IUserService
        {
            // Basic CRUD
            Task<UserResponseDto> GetByIdAsync(int id);
            Task<IEnumerable<UserResponseDto>> GetAllAsync();
            Task<UserResponseDto> CreateAsync(RegisterDto createDto);
            Task<UserResponseDto> UpdateAsync(int id, UpdateUserDto updateDto);
            Task<bool> DeleteAsync(int id);

            // User-specific methods
            Task<UserResponseDto> GetByEmailAsync(string email);
            Task<IEnumerable<UserResponseDto>> GetByRoleAsync(UserRole role);
            Task<bool> UpdateUserRoleAsync(UpdateUserRoleDto updateRoleDto);
            Task<UserResponseDto> GetUserWithVehiclesAsync(int userId);
            Task<bool> DeactivateUserAsync(int userId);
            Task<bool> ActivateUserAsync(int userId);
            Task<UserResponseDto> UpdateProfileAsync(int userId, UpdateUserDto updateUserDto);
        }
    
}
