using SmartParkingSystem.DTOs.User;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Interfaces.Services;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Basic CRUD methods
        public async Task<UserResponseDto> GetByIdAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                    throw new ArgumentException($"User with Id {id} not found.");

                return MapToUserResponseDto(user);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving user with Id {id}.", ex);
            }
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                return users.Select(MapToUserResponseDto);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all users.", ex);
            }
        }

        public async Task<UserResponseDto> CreateAsync(RegisterDto createDto)
        {
            try
            {
                // Check if email already exists
                if (await _userRepository.EmailExistsAsync(createDto.Email))
                    throw new InvalidOperationException("Email already exists.");

                var user = new User
                {
                    FirstName = createDto.FirstName,
                    LastName = createDto.LastName,
                    Email = createDto.Email,
                    PhoneNumber = createDto.PhoneNumber,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(createDto.Password),
                    Role = UserRole.User,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userRepository.AddAsync(user);
                return MapToUserResponseDto(createdUser);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating user.", ex);
            }
        }

        public async Task<UserResponseDto> UpdateAsync(int id, UpdateUserDto updateDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                    throw new ArgumentException($"User with Id {id} not found.");

                user.FirstName = updateDto.FirstName;
                user.LastName = updateDto.LastName;
                user.PhoneNumber = updateDto.PhoneNumber;
                user.ProfileImageUrl = updateDto.ProfileImageUrl;

                var updatedUser = await _userRepository.UpdateAsync(user);
                return MapToUserResponseDto(updatedUser);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating user with Id {id}.", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var exists = await _userRepository.ExistsAsync(id);

                if (!exists)
                    throw new ArgumentException($"User with Id {id} not found.");

                return await _userRepository.DeleteAsync(id);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting user with Id {id}.", ex);
            }
        }

        // User-specific methods
        public async Task<UserResponseDto> GetByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                    throw new ArgumentException($"User with email {email} not found.");

                return MapToUserResponseDto(user);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving user with email {email}.", ex);
            }
        }

        public async Task<IEnumerable<UserResponseDto>> GetByRoleAsync(UserRole role)
        {
            try
            {
                var users = await _userRepository.GetByRoleAsync(role);
                return users.Select(MapToUserResponseDto);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving users with role {role}.", ex);
            }
        }

        public async Task<bool> UpdateUserRoleAsync(UpdateUserRoleDto updateRoleDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(updateRoleDto.UserId);

                if (user == null)
                    throw new ArgumentException($"User with Id {updateRoleDto.UserId} not found.");

                user.Role = updateRoleDto.Role;

                await _userRepository.UpdateAsync(user);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating user role for Id {updateRoleDto.UserId}.", ex);
            }
        }

        public async Task<UserResponseDto> GetUserWithVehiclesAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserWithVehiclesAsync(userId);

                if (user == null)
                    throw new ArgumentException($"User with Id {userId} not found.");

                return MapToUserResponseDto(user);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving user with vehicles for Id {userId}.", ex);
            }
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                    throw new ArgumentException($"User with Id {userId} not found.");

                user.IsActive = false;

                await _userRepository.UpdateAsync(user);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deactivating user with Id {userId}.", ex);
            }
        }

        public async Task<bool> ActivateUserAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                    throw new ArgumentException($"User with Id {userId} not found.");

                user.IsActive = true;

                await _userRepository.UpdateAsync(user);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error activating user with Id {userId}.", ex);
            }
        }

        public async Task<UserResponseDto> UpdateProfileAsync(int userId, UpdateUserDto updateUserDto)
        {
            try
            {
                return await UpdateAsync(userId, updateUserDto);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating profile for user Id {userId}.", ex);
            }
        }

        // Private mapping method
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
