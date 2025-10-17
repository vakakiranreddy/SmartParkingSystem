using SmartParkingSystem.DTOs.Vehicle;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Interfaces.Services;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IUserRepository _userRepository;

        public VehicleService(IVehicleRepository vehicleRepository, IUserRepository userRepository)
        {
            _vehicleRepository = vehicleRepository;
            _userRepository = userRepository;
        }

        // Basic CRUD methods
        public async Task<VehicleResponseDto> GetByIdAsync(int id)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(id);

                if (vehicle == null)
                    throw new ArgumentException($"Vehicle with Id {id} not found.");

                return await MapToVehicleResponseDto(vehicle);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving vehicle with Id {id}.", ex);
            }
        }

        public async Task<IEnumerable<VehicleResponseDto>> GetAllAsync()
        {
            try
            {
                var vehicles = await _vehicleRepository.GetAllAsync();
                var vehicleDtos = new List<VehicleResponseDto>();

                foreach (var vehicle in vehicles)
                {
                    var dto = await MapToVehicleResponseDto(vehicle);
                    vehicleDtos.Add(dto);
                }

                return vehicleDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all vehicles.", ex);
            }
        }

       

        public async Task<VehicleResponseDto> CreateAsync(CreateVehicleDto createDto, int ownerId, byte[] vehicleImage = null)
        {
           
            if (await _vehicleRepository.LicensePlateExistsAsync(createDto.LicensePlate))
                throw new InvalidOperationException($"License plate {createDto.LicensePlate} already exists.");

            var vehicle = new Vehicle
            {
                LicensePlate = createDto.LicensePlate,
                VehicleType = createDto.VehicleType,
                Brand = createDto.Brand,
                Model = createDto.Model,
                Color = createDto.Color,
                VehicleImage = vehicleImage,
                OwnerId = ownerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdVehicle = await _vehicleRepository.AddAsync(vehicle);
            return await MapToVehicleResponseDto(createdVehicle);
        }

        

        public async Task<VehicleResponseDto> UpdateAsync(int id, UpdateVehicleDto updateDto, byte[] vehicleImage = null)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(id);

                if (vehicle == null)
                    throw new ArgumentException($"Vehicle with Id {id} not found.");

                
                var existingVehicle = await _vehicleRepository.GetByLicensePlateAsync(updateDto.LicensePlate);
                if (existingVehicle != null && existingVehicle.Id != id)
                    throw new InvalidOperationException($"License plate {updateDto.LicensePlate} already exists.");

                vehicle.LicensePlate = updateDto.LicensePlate;
                vehicle.VehicleType = updateDto.VehicleType;
                vehicle.Brand = updateDto.Brand;
                vehicle.Model = updateDto.Model;
                vehicle.Color = updateDto.Color;

                if (vehicleImage != null)
                    vehicle.VehicleImage = vehicleImage;

                var updatedVehicle = await _vehicleRepository.UpdateAsync(vehicle);
                return await MapToVehicleResponseDto(updatedVehicle);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating vehicle with Id {id}.", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var exists = await _vehicleRepository.ExistsAsync(id);

                if (!exists)
                    throw new ArgumentException($"Vehicle with Id {id} not found.");

                return await _vehicleRepository.DeleteAsync(id);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting vehicle with Id {id}.", ex);
            }
        }

        // Vehicle-specific methods
        public async Task<IEnumerable<VehicleResponseDto>> GetUserVehiclesAsync(int userId)
        {
            try
            {
                var vehicles = await _vehicleRepository.GetByOwnerIdAsync(userId);
                var vehicleDtos = new List<VehicleResponseDto>();

                foreach (var vehicle in vehicles)
                {
                    var dto = await MapToVehicleResponseDto(vehicle);
                    vehicleDtos.Add(dto);
                }

                return vehicleDtos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving vehicles for user Id {userId}.", ex);
            }
        }

        public async Task<VehicleResponseDto> GetByLicensePlateAsync(string licensePlate)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByLicensePlateAsync(licensePlate);

                if (vehicle == null)
                    throw new ArgumentException($"Vehicle with license plate {licensePlate} not found.");

                return await MapToVehicleResponseDto(vehicle);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving vehicle with license plate {licensePlate}.", ex);
            }
        }

       

        private async Task<VehicleResponseDto> MapToVehicleResponseDto(Vehicle vehicle)
        {
            var dto = new VehicleResponseDto
            {
                Id = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                VehicleType = vehicle.VehicleType,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Color = vehicle.Color,
                VehicleImageBase64 = vehicle.VehicleImage != null ? Convert.ToBase64String(vehicle.VehicleImage) : null,
                OwnerId = vehicle.OwnerId,
                OwnerName = "",
                IsActive = vehicle.IsActive,
                CreatedAt = vehicle.CreatedAt
            };

            
            try
            {
                var user = await _userRepository.GetByIdAsync(vehicle.OwnerId);
                if (user != null)
                {
                    dto.OwnerName = $"{user.FirstName} {user.LastName}";
                }
            }
            catch
            {
                dto.OwnerName = "Unknown Owner";
            }

            return dto;
        }
    }
}
