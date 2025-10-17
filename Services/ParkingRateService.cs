using SmartParkingSystem.DTOs.ParkingRate;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Interfaces.Services;
using SmartParkingSystem.Models;

public class ParkingRateService : IParkingRateService
{
    private readonly IParkingRateRepository _parkingRateRepository;

    public ParkingRateService(IParkingRateRepository parkingRateRepository)
    {
        _parkingRateRepository = parkingRateRepository;
    }

   
    public async Task<ParkingRateResponseDto> GetByIdAsync(int id)
    {
        try
        {
            var rate = await _parkingRateRepository.GetByIdAsync(id);

            if (rate == null)
                throw new ArgumentException($"Parking rate with Id {id} not found.");

            return MapToParkingRateResponseDto(rate);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving parking rate with Id {id}.", ex);
        }
    }

    public async Task<IEnumerable<ParkingRateResponseDto>> GetAllAsync()
    {
        try
        {
            var rates = await _parkingRateRepository.GetAllAsync();
            return rates.Select(MapToParkingRateResponseDto);
        }
        catch (Exception ex)
        {
            throw new Exception("Error retrieving all parking rates.", ex);
        }
    }

    public async Task<ParkingRateResponseDto> CreateAsync(CreateParkingRateDto createDto)
    {
        try
        {
            
            var existingRate = await _parkingRateRepository.GetByVehicleTypeAsync(createDto.VehicleType);
            if (existingRate != null)
                throw new InvalidOperationException($"Active parking rate for vehicle type '{createDto.VehicleType}' already exists.");

            var rate = new ParkingRate
            {
                VehicleType = createDto.VehicleType,
                HourlyRate = createDto.HourlyRate,
                DailyRate = createDto.DailyRate,
                IsActive = true
            };

            var createdRate = await _parkingRateRepository.AddAsync(rate);
            return MapToParkingRateResponseDto(createdRate);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("Error creating parking rate.", ex);
        }
    }

    public async Task<ParkingRateResponseDto> UpdateAsync(int id, UpdateParkingRateDto updateDto)
    {
        try
        {
            var rate = await _parkingRateRepository.GetByIdAsync(id);

            if (rate == null)
                throw new ArgumentException($"Parking rate with Id {id} not found.");

            // Check if another active rate for this vehicle type exists (excluding current rate)
            var existingRate = await _parkingRateRepository.GetByVehicleTypeAsync(updateDto.VehicleType);
            if (existingRate != null && existingRate.Id != id)
                throw new InvalidOperationException($"Another active parking rate for vehicle type '{updateDto.VehicleType}' already exists.");

            rate.VehicleType = updateDto.VehicleType;
            rate.HourlyRate = updateDto.HourlyRate;
            rate.DailyRate = updateDto.DailyRate;
            rate.IsActive = updateDto.IsActive;

            var updatedRate = await _parkingRateRepository.UpdateAsync(rate);
            return MapToParkingRateResponseDto(updatedRate);
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
            throw new Exception($"Error updating parking rate with Id {id}.", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var exists = await _parkingRateRepository.ExistsAsync(id);

            if (!exists)
                throw new ArgumentException($"Parking rate with Id {id} not found.");

            return await _parkingRateRepository.DeleteAsync(id);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting parking rate with Id {id}.", ex);
        }
    }

   
    public async Task<ParkingRateResponseDto> GetRateByVehicleTypeAsync(VehicleType vehicleType)
    {
        try
        {
            var rate = await _parkingRateRepository.GetByVehicleTypeAsync(vehicleType);

            if (rate == null)
                throw new ArgumentException($"No active parking rate found for vehicle type '{vehicleType}'.");

            return MapToParkingRateResponseDto(rate);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving parking rate for vehicle type '{vehicleType}'.", ex);
        }
    }

    public async Task<IEnumerable<ParkingRateResponseDto>> GetActiveRatesAsync()
    {
        try
        {
            var rates = await _parkingRateRepository.GetActiveRatesAsync();
            return rates.Select(MapToParkingRateResponseDto);
        }
        catch (Exception ex)
        {
            throw new Exception("Error retrieving active parking rates.", ex);
        }
    }

   
    private ParkingRateResponseDto MapToParkingRateResponseDto(ParkingRate rate)
    {
        return new ParkingRateResponseDto
        {
            Id = rate.Id,
            VehicleType = rate.VehicleType,
            HourlyRate = rate.HourlyRate,
            DailyRate = rate.DailyRate,
            IsActive = rate.IsActive
        };
    }
}