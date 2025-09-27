//using SmartParkingSystem.DTOs.User;
//using SmartParkingSystem.Interfaces.Repositories;
//using SmartParkingSystem.Interfaces.Services;
//using SmartParkingSystem.Models;

//namespace SmartParkingSystem.Services
//{
//    public class AuthService : IAuthService
//    {
//        private readonly IUserRepository _userRepository;
//        private readonly ITokenService _tokenService;
//        private readonly IEmailNotificationService _emailNotificationService;

//        public AuthService(IUserRepository userRepository, ITokenService tokenService, IEmailNotificationService emailNotificationService)
//        {
//            _userRepository = userRepository;
//            _tokenService = tokenService;
//            _emailNotificationService = emailNotificationService;
//        }

//        //public async Task<string> LoginAsync(LoginDto loginDto)
//        //{
//        //    try
//        //    {
//        //        var user = await _userRepository.GetByEmailAsync(loginDto.Email);

//        //        if (user == null)
//        //            throw new UnauthorizedAccessException("Invalid email or password.");

//        //        if (!user.IsActive)
//        //            throw new UnauthorizedAccessException("Account is deactivated.");

//        //        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
//        //            throw new UnauthorizedAccessException("Invalid email or password.");

//        //        return _tokenService.GenerateJwtToken(user);
//        //    }
//        //    catch (UnauthorizedAccessException)
//        //    {
//        //        throw;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        throw new Exception("Error during login.", ex);
//        //    }
//        //}

//        public async Task<(string Token, UserResponseDto User)> LoginAsync(LoginDto loginDto)
//        {
//            try
//            {
//                var dbUser = await _userRepository.GetByEmailAsync(loginDto.Email);

//                if (dbUser == null)
//                    throw new UnauthorizedAccessException("Invalid email or password.");

//                if (!dbUser.IsActive)
//                    throw new UnauthorizedAccessException("Account is deactivated.");

//                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, dbUser.PasswordHash))
//                    throw new UnauthorizedAccessException("Invalid email or password.");

//                var generatedToken = _tokenService.GenerateJwtToken(dbUser);
//                var userResponse = MapToUserResponseDto(dbUser);

//                return (Token: generatedToken, User: userResponse);
//            }
//            catch (UnauthorizedAccessException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error during login.", ex);
//            }
//        }

//        public async Task<UserResponseDto> RegisterAsync(RegisterDto registerDto)
//        {
//            try
//            {
//                // Check if email already exists
//                if (await _userRepository.EmailExistsAsync(registerDto.Email))
//                    throw new InvalidOperationException("Email already exists.");

//                // Create new user
//                var user = new User
//                {
//                    FirstName = registerDto.FirstName,
//                    LastName = registerDto.LastName,
//                    Email = registerDto.Email,
//                    PhoneNumber = registerDto.PhoneNumber,
//                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
//                    Role = UserRole.User, // Default role
//                    IsActive = true,
//                    CreatedAt = DateTime.UtcNow
//                };

//                var createdUser = await _userRepository.AddAsync(user);

//                await Task.Run(async () =>
//                {
//                    var emailDto = new DTOs.EmailNotification.SendEmailNotificationDto
//                    {
//                        UserId = createdUser.Id,
//                        ParkingSessionId = 0, // Not linked to a session
//                        EmailAddress = createdUser.Email,
//                        Subject = "Welcome to Smart Parking System 🚗",
//                        Message = $"Dear {createdUser.FirstName},\n\n" +
//                                  "🎉 Congratulations! Your account has been successfully registered in Smart Parking System.\n\n" +
//                                  "Now you can:\n" +
//                                  "✅ Reserve parking slots online\n" +
//                                  "✅ Manage your vehicles\n" +
//                                  "✅ Get reminders & payment updates\n\n" +
//                                  "Thank you for joining us!\n" +
//                                  "- Smart Parking System Team",
//                        NotificationType = NotificationType.General
//                    };
//                    await _emailNotificationService.SendNotificationAsync(emailDto);
//                });


//                return MapToUserResponseDto(createdUser);
//            }
//            catch (InvalidOperationException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception($"Error during registration: {ex.InnerException?.Message ?? ex.Message}", ex);
//            }
//        }

//        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
//        {
//            try
//            {
//                var user = await _userRepository.GetByIdAsync(userId);

//                if (user == null)
//                    throw new ArgumentException("User not found.");

//                if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
//                    throw new UnauthorizedAccessException("Current password is incorrect.");

//                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

//                await _userRepository.UpdateAsync(user);
//                return true;
//            }
//            catch (ArgumentException)
//            {
//                throw;
//            }
//            catch (UnauthorizedAccessException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error changing password.", ex);
//            }
//        }

//        public async Task<UserResponseDto> GetCurrentUserAsync(int userId)
//        {
//            try
//            {
//                var user = await _userRepository.GetByIdAsync(userId);

//                if (user == null)
//                    throw new ArgumentException("User not found.");

//                return MapToUserResponseDto(user);
//            }
//            catch (ArgumentException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error retrieving current user.", ex);
//            }
//        }

//        private UserResponseDto MapToUserResponseDto(User user)
//        {
//            return new UserResponseDto
//            {
//                Id = user.Id,
//                FirstName = user.FirstName,
//                LastName = user.LastName,
//                Email = user.Email,
//                PhoneNumber = user.PhoneNumber,
//                Role = user.Role,
//                ProfileImageUrl = user.ProfileImageUrl,
//                IsActive = user.IsActive,
//                CreatedAt = user.CreatedAt
//            };
//        }
//    }
//}
//using SmartParkingSystem.DTOs.User;
//using SmartParkingSystem.Interfaces.Repositories;
//using SmartParkingSystem.Interfaces.Services;
//using SmartParkingSystem.Models;
//using System.Net;
//using System.Net.Mail;

//namespace SmartParkingSystem.Services
//{
//    public class AuthService : IAuthService
//    {
//        private readonly IUserRepository _userRepository;
//        private readonly ITokenService _tokenService;
//        private readonly IEmailNotificationService _emailNotificationService;
//        private readonly IConfiguration _configuration;

//        public AuthService(IUserRepository userRepository, ITokenService tokenService,
//            IEmailNotificationService emailNotificationService, IConfiguration configuration)
//        {
//            _userRepository = userRepository;
//            _tokenService = tokenService;
//            _emailNotificationService = emailNotificationService;
//            _configuration = configuration;
//        }

//        public async Task<(string Token, UserResponseDto User)> LoginAsync(LoginDto loginDto)
//        {
//            try
//            {
//                var dbUser = await _userRepository.GetByEmailAsync(loginDto.Email);

//                if (dbUser == null)
//                    throw new UnauthorizedAccessException("Invalid email or password.");

//                if (!dbUser.IsActive)
//                    throw new UnauthorizedAccessException("Account is deactivated.");

//                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, dbUser.PasswordHash))
//                    throw new UnauthorizedAccessException("Invalid email or password.");

//                var generatedToken = _tokenService.GenerateJwtToken(dbUser);
//                var userResponse = MapToUserResponseDto(dbUser);

//                return (Token: generatedToken, User: userResponse);
//            }
//            catch (UnauthorizedAccessException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error during login.", ex);
//            }
//        }

//        public async Task<UserResponseDto> RegisterAsync(RegisterDto registerDto)
//        {
//            try
//            {
//                // Check if email already exists
//                if (await _userRepository.EmailExistsAsync(registerDto.Email))
//                    throw new InvalidOperationException("Email already exists.");

//                // Create new user
//                var user = new User
//                {
//                    FirstName = registerDto.FirstName,
//                    LastName = registerDto.LastName,
//                    Email = registerDto.Email,
//                    PhoneNumber = registerDto.PhoneNumber,
//                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
//                    Role = UserRole.User,
//                    IsActive = true,
//                    CreatedAt = DateTime.UtcNow
//                };

//                var createdUser = await _userRepository.AddAsync(user);

//                // Send welcome email directly
//                try
//                {
//                    await SendWelcomeEmailAsync(createdUser);
//                    Console.WriteLine($"Welcome email sent successfully to {createdUser.Email}");
//                }
//                catch (Exception emailEx)
//                {
//                    // Log email error but don't fail registration
//                    Console.WriteLine($"Failed to send welcome email: {emailEx.Message}");
//                }

//                return MapToUserResponseDto(createdUser);
//            }
//            catch (InvalidOperationException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception($"Error during registration: {ex.InnerException?.Message ?? ex.Message}", ex);
//            }
//        }

//        // Direct email method - no database dependency
//        private async Task SendWelcomeEmailAsync(User user)
//        {
//            try
//            {
//                var emailSettings = _configuration.GetSection("EmailSettings");
//                var smtpHost = emailSettings["SmtpHost"];
//                var smtpPort = int.Parse(emailSettings["SmtpPort"]);
//                var fromEmail = emailSettings["FromEmail"];
//                var fromName = emailSettings["FromName"];
//                var password = emailSettings["Password"];
//                var enableSsl = bool.Parse(emailSettings["EnableSsl"]);

//                using var client = new SmtpClient(smtpHost, smtpPort)
//                {
//                    EnableSsl = enableSsl,
//                    Credentials = new NetworkCredential(fromEmail, password)
//                };

//                var subject = "Welcome to Smart Parking System 🚗";
//                var message = $"Dear {user.FirstName},\n\n" +
//                            "🎉 Congratulations! Your account has been successfully registered in Smart Parking System.\n\n" +
//                            "Now you can:\n" +
//                            "✅ Reserve parking slots online\n" +
//                            "✅ Manage your vehicles\n" +
//                            "✅ Get reminders & payment updates\n\n" +
//                            "Thank you for joining us!\n" +
//                            "- Smart Parking System Team";

//                var mailMessage = new MailMessage
//                {
//                    From = new MailAddress(fromEmail, fromName),
//                    Subject = subject,
//                    Body = message,
//                    IsBodyHtml = false
//                };

//                mailMessage.To.Add(user.Email);
//                await client.SendMailAsync(mailMessage);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"SMTP Error: {ex.Message}");
//                throw;
//            }
//        }

//        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
//        {
//            try
//            {
//                var user = await _userRepository.GetByIdAsync(userId);

//                if (user == null)
//                    throw new ArgumentException("User not found.");

//                if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
//                    throw new UnauthorizedAccessException("Current password is incorrect.");

//                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

//                await _userRepository.UpdateAsync(user);
//                return true;
//            }
//            catch (ArgumentException)
//            {
//                throw;
//            }
//            catch (UnauthorizedAccessException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error changing password.", ex);
//            }
//        }

//        public async Task<UserResponseDto> GetCurrentUserAsync(int userId)
//        {
//            try
//            {
//                var user = await _userRepository.GetByIdAsync(userId);

//                if (user == null)
//                    throw new ArgumentException("User not found.");

//                return MapToUserResponseDto(user);
//            }
//            catch (ArgumentException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error retrieving current user.", ex);
//            }
//        }

//        private UserResponseDto MapToUserResponseDto(User user)
//        {
//            return new UserResponseDto
//            {
//                Id = user.Id,
//                FirstName = user.FirstName,
//                LastName = user.LastName,
//                Email = user.Email,
//                PhoneNumber = user.PhoneNumber,
//                Role = user.Role,
//                ProfileImageUrl = user.ProfileImageUrl,
//                IsActive = user.IsActive,
//                CreatedAt = user.CreatedAt
//            };
//        }
//    }
//}

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
                    //await SendLoginNotificationEmailAsync(dbUser);
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
                    await SendWelcomeEmailAsync(createdUser);
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

        // Direct email method - no database dependency
        private async Task SendWelcomeEmailAsync(User user)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = emailSettings["SmtpHost"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"]);
                var fromEmail = emailSettings["FromEmail"];
                var fromName = emailSettings["FromName"];
                var password = emailSettings["Password"];
                var enableSsl = bool.Parse(emailSettings["EnableSsl"]);

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = enableSsl,
                    Credentials = new NetworkCredential(fromEmail, password)
                };

                var subject = "Welcome to Smart Parking System 🚗";
                var message = $"Dear {user.FirstName},\n\n" +
                            "🎉 Congratulations! Your account has been successfully registered in Smart Parking System.\n\n" +
                            "Now you can:\n" +
                            "✅ Reserve parking slots online\n" +
                            "✅ Manage your vehicles\n" +
                            "✅ Get reminders & payment updates\n\n" +
                            "Thank you for joining us!\n" +
                            "- Smart Parking System Team";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(user.Email);
                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMTP Error: {ex.Message}");
                throw;
            }
        }

        // Send login notification email
        private async Task SendLoginNotificationEmailAsync(User user)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = emailSettings["SmtpHost"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"]);
                var fromEmail = emailSettings["FromEmail"];
                var fromName = emailSettings["FromName"];
                var password = emailSettings["Password"];
                var enableSsl = bool.Parse(emailSettings["EnableSsl"]);

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = enableSsl,
                    Credentials = new NetworkCredential(fromEmail, password)
                };

                var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var loginTimeIst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istTimeZone);

                var subject = "Login Alert - Smart Parking System";
                var message = $"Dear {user.FirstName},\n\n" +
                            "🔐 Your account was accessed successfully.\n\n" +
                            "Login Details:\n" +
                            $"• Time: {loginTimeIst:dd-MM-yyyy HH:mm} IST\n" +
                            $"• Account: {user.Email}\n\n" +
                            "If this wasn't you, please change your password immediately.\n\n" +
                            "Stay secure!\n" +
                            "- Smart Parking System Team";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(user.Email);
                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMTP Error (Login notification): {ex.Message}");
                throw;
            }
        }

        // Send password change notification email
        private async Task SendPasswordChangeNotificationEmailAsync(User user)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = emailSettings["SmtpHost"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"]);
                var fromEmail = emailSettings["FromEmail"];
                var fromName = emailSettings["FromName"];
                var password = emailSettings["Password"];
                var enableSsl = bool.Parse(emailSettings["EnableSsl"]);

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = enableSsl,
                    Credentials = new NetworkCredential(fromEmail, password)
                };

                var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var changeTimeIst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istTimeZone);

                var subject = "Password Changed Successfully - Smart Parking System";
                var message = $"Dear {user.FirstName},\n\n" +
                            "🔑 Your password has been changed successfully.\n\n" +
                            "Change Details:\n" +
                            $"• Time: {changeTimeIst:dd-MM-yyyy HH:mm} IST\n" +
                            $"• Account: {user.Email}\n\n" +
                            "If you didn't make this change, please contact support immediately.\n\n" +
                            "Your account security is important to us!\n" +
                            "- Smart Parking System Team";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(user.Email);
                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMTP Error (Password change notification): {ex.Message}");
                throw;
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
                    await SendPasswordChangeNotificationEmailAsync(user);
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
                ProfileImageUrl = user.ProfileImageUrl,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}