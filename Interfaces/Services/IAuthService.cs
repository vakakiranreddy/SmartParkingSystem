using SmartParkingSystem.DTOs.User;

namespace SmartParkingSystem.Interfaces.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginDto loginDto);
        Task<UserResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<UserResponseDto> GetCurrentUserAsync(int userId);
    }
}
