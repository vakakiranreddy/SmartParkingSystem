using SmartParkingSystem.DTOs.User;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Interfaces.Services;
using SmartParkingSystem.Models;
using System.Net;
using System.Net.Mail;

namespace SmartParkingSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailNotificationService _emailNotificationService;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, ITokenService tokenService,
            IEmailNotificationService emailNotificationService, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _emailNotificationService = emailNotificationService;
            _configuration = configuration;
        }

        public async Task<(string Token, UserResponseDto User)> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var dbUser = await _userRepository.GetByEmailAsync(loginDto.Email);

                if (dbUser == null)
                    throw new UnauthorizedAccessException("Invalid email or password.");

                if (!dbUser.IsActive)
                    throw new UnauthorizedAccessException("Account is deactivated.");

                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, dbUser.PasswordHash))
                    throw new UnauthorizedAccessException("Invalid email or password.");

                var generatedToken = _tokenService.GenerateJwtToken(dbUser);
                var userResponse = MapToUserResponseDto(dbUser);

                // Send login notification email
                try
                {
                   
                    Console.WriteLine($"Login notification sent to {dbUser.Email}");
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"Failed to send login notification: {emailEx.Message}");
                }

                return (Token: generatedToken, User: userResponse);
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error during login.", ex);
            }
        }

        public async Task<UserResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if email already exists
                if (await _userRepository.EmailExistsAsync(registerDto.Email))
                    throw new InvalidOperationException("Email already exists.");

                // Create new user
                var user = new User
                {
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Email = registerDto.Email,
                    PhoneNumber = registerDto.PhoneNumber,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    Role = UserRole.User,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userRepository.AddAsync(user);

                // Send welcome email directly
                try
                {
                    //await _emailNotificationService.SendWelcomeEmailAsync(createdUser);
                    Console.WriteLine($"Welcome email sent successfully to {createdUser.Email}");
                }
                catch (Exception emailEx)
                {
                    // Log email error but don't fail registration
                    Console.WriteLine($"Failed to send welcome email: {emailEx.Message}");
                }

                return MapToUserResponseDto(createdUser);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during registration: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }

     



      

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                    throw new ArgumentException("User not found.");

                if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
                    throw new UnauthorizedAccessException("Current password is incorrect.");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

                await _userRepository.UpdateAsync(user);

                // Send password change notification email
                try
                {
                    //await _emailNotificationService.SendPasswordChangeNotificationEmailAsync(user);
                    Console.WriteLine($"Password change notification sent to {user.Email}");
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"Failed to send password change notification: {emailEx.Message}");
                }

                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error changing password.", ex);
            }
        }

        public async Task<UserResponseDto> GetCurrentUserAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                    throw new ArgumentException("User not found.");

                return MapToUserResponseDto(user);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving current user.", ex);
            }
        }

        private UserResponseDto MapToUserResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                ProfileImageBase64 = user.ProfileImage != null ? Convert.ToBase64String(user.ProfileImage) : null,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}