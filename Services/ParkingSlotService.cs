using SmartParkingSystem.DTOs.ParkingSlot;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Interfaces.Services;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Services
{
    public class ParkingSlotService : IParkingSlotService
    {
        private readonly IParkingSlotRepository _parkingSlotRepository;
        private readonly IParkingSessionRepository _parkingSessionRepository;

        public ParkingSlotService(IParkingSlotRepository parkingSlotRepository, IParkingSessionRepository parkingSessionRepository)
        {
            _parkingSlotRepository = parkingSlotRepository;
            _parkingSessionRepository = parkingSessionRepository;
        }

        // Basic CRUD methods
        public async Task<ParkingSlotResponseDto> GetByIdAsync(int id)
        {
            try
            {
                var slot = await _parkingSlotRepository.GetByIdAsync(id);

                if (slot == null)
                    throw new ArgumentException($"Parking slot with Id {id} not found.");

                return await MapToParkingSlotResponseDto(slot);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving parking slot with Id {id}.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSlotResponseDto>> GetAllAsync()
        {
            try
            {
                var slots = await _parkingSlotRepository.GetAllAsync();
                var slotDtos = new List<ParkingSlotResponseDto>();

                foreach (var slot in slots)
                {
                    var dto = await MapToParkingSlotResponseDto(slot);
                    slotDtos.Add(dto);
                }

                return slotDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all parking slots.", ex);
            }
        }

        //public async Task<ParkingSlotResponseDto> CreateAsync(CreateParkingSlotDto createDto)
        //{
        //    try
        //    {
        //        // Check if slot number already exists
        //        if (await _parkingSlotRepository.SlotNumberExistsAsync(createDto.SlotNumber))
        //            throw new InvalidOperationException($"Slot number '{createDto.SlotNumber}' already exists.");

        //        var slot = new ParkingSlot
        //        {
        //            SlotNumber = createDto.SlotNumber,
        //            Floor = createDto.Floor,
        //            Section = createDto.Section,
        //            SlotImage = createDto.SlotImage,
        //            IsOccupied = false,
        //            IsActive = true
        //        };

        //        var createdSlot = await _parkingSlotRepository.AddAsync(slot);
        //        return await MapToParkingSlotResponseDto(createdSlot);
        //    }
        //    catch (InvalidOperationException)
        //    {
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error creating parking slot.", ex);
        //    }
        //}

        public async Task<ParkingSlotResponseDto> CreateAsync(CreateParkingSlotDto createDto)
        {
            try
            {
                if (await _parkingSlotRepository.SlotNumberExistsAsync(createDto.SlotNumber))
                    throw new InvalidOperationException($"Slot number '{createDto.SlotNumber}' already exists.");

                var slot = new ParkingSlot
                {
                    SlotNumber = createDto.SlotNumber,
                    Floor = createDto.Floor,
                    Section = createDto.Section,
                    SlotImage = ConvertBase64ToByteArray(createDto.SlotImageBase64), // CHANGED THIS LINE
                    IsOccupied = false,
                    IsActive = true
                };

                var createdSlot = await _parkingSlotRepository.AddAsync(slot);
                return await MapToParkingSlotResponseDto(createdSlot);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating parking slot.", ex);
            }
        }

        //public async Task<ParkingSlotResponseDto> UpdateAsync(int id, UpdateParkingSlotDto updateDto)
        //{
        //    try
        //    {
        //        var slot = await _parkingSlotRepository.GetByIdAsync(id);

        //        if (slot == null)
        //            throw new ArgumentException($"Parking slot with Id {id} not found.");

        //        // Check if slot number already exists (excluding current slot)
        //        var existingSlot = await _parkingSlotRepository.GetAllAsync();
        //        var duplicateSlot = existingSlot.FirstOrDefault(s => s.SlotNumber == updateDto.SlotNumber && s.Id != id);
        //        if (duplicateSlot != null)
        //            throw new InvalidOperationException($"Slot number '{updateDto.SlotNumber}' already exists.");

        //        slot.SlotNumber = updateDto.SlotNumber;
        //        slot.Floor = updateDto.Floor;
        //        slot.Section = updateDto.Section;
        //        slot.SlotImage = updateDto.SlotImage;
        //        slot.IsActive = updateDto.IsActive;

        //        var updatedSlot = await _parkingSlotRepository.UpdateAsync(slot);
        //        return await MapToParkingSlotResponseDto(updatedSlot);
        //    }
        //    catch (ArgumentException)
        //    {
        //        throw;
        //    }
        //    catch (InvalidOperationException)
        //    {
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error updating parking slot with Id {id}.", ex);
        //    }
        //}

        public async Task<ParkingSlotResponseDto> UpdateAsync(int id, UpdateParkingSlotDto updateDto)
        {
            try
            {
                var slot = await _parkingSlotRepository.GetByIdAsync(id);

                if (slot == null)
                    throw new ArgumentException($"Parking slot with Id {id} not found.");

                var existingSlot = await _parkingSlotRepository.GetAllAsync();
                var duplicateSlot = existingSlot.FirstOrDefault(s => s.SlotNumber == updateDto.SlotNumber && s.Id != id);
                if (duplicateSlot != null)
                    throw new InvalidOperationException($"Slot number '{updateDto.SlotNumber}' already exists.");

                slot.SlotNumber = updateDto.SlotNumber;
                slot.Floor = updateDto.Floor;
                slot.Section = updateDto.Section;
                slot.SlotImage = ConvertBase64ToByteArray(updateDto.SlotImageBase64); // CHANGED THIS LINE
                slot.IsActive = updateDto.IsActive;

                var updatedSlot = await _parkingSlotRepository.UpdateAsync(slot);
                return await MapToParkingSlotResponseDto(updatedSlot);
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
                throw new Exception($"Error updating parking slot with Id {id}.", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var exists = await _parkingSlotRepository.ExistsAsync(id);

                if (!exists)
                    throw new ArgumentException($"Parking slot with Id {id} not found.");

                return await _parkingSlotRepository.DeleteAsync(id);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting parking slot with Id {id}.", ex);
            }
        }

        // Slot-specific methods
        public async Task<IEnumerable<ParkingSlotResponseDto>> GetAvailableSlotsAsync()
        {
            try
            {
                var slots = await _parkingSlotRepository.GetAvailableSlotsAsync();
                var slotDtos = new List<ParkingSlotResponseDto>();

                foreach (var slot in slots)
                {
                    var dto = await MapToParkingSlotResponseDto(slot);
                    slotDtos.Add(dto);
                }

                return slotDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving available parking slots.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSlotResponseDto>> SearchSlotsAsync(SlotSearchDto searchDto)
        {
            try
            {
                var slots = await _parkingSlotRepository.SearchSlotsAsync(
                    searchDto.Floor,
                    searchDto.Section,
                    searchDto.IsOccupied);

                var slotDtos = new List<ParkingSlotResponseDto>();

                foreach (var slot in slots)
                {
                    var dto = await MapToParkingSlotResponseDto(slot);
                    slotDtos.Add(dto);
                }

                return slotDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching parking slots.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSlotResponseDto>> GetSlotsByFloorAsync(string floor)
        {
            try
            {
                var slots = await _parkingSlotRepository.GetByFloorAsync(floor);
                var slotDtos = new List<ParkingSlotResponseDto>();

                foreach (var slot in slots)
                {
                    var dto = await MapToParkingSlotResponseDto(slot);
                    slotDtos.Add(dto);
                }

                return slotDtos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving slots for floor '{floor}'.", ex);
            }
        }

        public async Task<ParkingSlotResponseDto> GetSlotWithFeaturesAsync(int slotId)
        {
            try
            {
                var slot = await _parkingSlotRepository.GetSlotWithFeaturesAsync(slotId);

                if (slot == null)
                    throw new ArgumentException($"Parking slot with Id {slotId} not found.");

                return await MapToParkingSlotResponseDto(slot);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving slot with features for Id {slotId}.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSlotResponseDto>> GetSlotsByFeatureAsync(int featureId)
        {
            try
            {
                var slots = await _parkingSlotRepository.GetSlotsByFeatureAsync(featureId);
                var slotDtos = new List<ParkingSlotResponseDto>();

                foreach (var slot in slots)
                {
                    var dto = await MapToParkingSlotResponseDto(slot);
                    slotDtos.Add(dto);
                }

                return slotDtos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving slots with feature Id {featureId}.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSlotResponseDto>> GetSlotsWithFeaturesAsync(List<int> featureIds)
        {
            try
            {
                var slots = await _parkingSlotRepository.GetSlotsWithFeaturesAsync(featureIds);
                var slotDtos = new List<ParkingSlotResponseDto>();

                foreach (var slot in slots)
                {
                    var dto = await MapToParkingSlotResponseDto(slot);
                    slotDtos.Add(dto);
                }

                return slotDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving slots with specified features.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSlotResponseDto>> GetAvailableSlotsWithFeaturesAsync(List<int> featureIds)
        {
            try
            {
                var slots = await _parkingSlotRepository.GetAvailableSlotsWithFeaturesAsync(featureIds);
                var slotDtos = new List<ParkingSlotResponseDto>();

                foreach (var slot in slots)
                {
                    var dto = await MapToParkingSlotResponseDto(slot);
                    slotDtos.Add(dto);
                }

                return slotDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving available slots with specified features.", ex);
            }
        }

        public async Task<bool> BulkUpdateSlotStatusAsync(List<int> slotIds, bool isActive)
        {
            try
            {
                foreach (var slotId in slotIds)
                {
                    var slot = await _parkingSlotRepository.GetByIdAsync(slotId);
                    if (slot != null)
                    {
                        slot.IsActive = isActive;
                        await _parkingSlotRepository.UpdateAsync(slot);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error bulk updating slot status.", ex);
            }
        }

        public async Task<bool> BulkUpdateSlotOccupancyAsync(List<int> slotIds, bool isOccupied)
        {
            try
            {
                foreach (var slotId in slotIds)
                {
                    var slot = await _parkingSlotRepository.GetByIdAsync(slotId);
                    if (slot != null)
                    {
                        slot.IsOccupied = isOccupied;
                        await _parkingSlotRepository.UpdateAsync(slot);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error bulk updating slot occupancy.", ex);
            }
        }

        // Private mapping method
        //private async Task<ParkingSlotResponseDto> MapToParkingSlotResponseDto(ParkingSlot slot)
        //{
        //    var dto = new ParkingSlotResponseDto
        //    {
        //        Id = slot.Id,
        //        SlotNumber = slot.SlotNumber,
        //        Floor = slot.Floor,
        //        Section = slot.Section,
        //        SlotImage = slot.SlotImage,
        //        IsOccupied = slot.IsOccupied,
        //        IsActive = slot.IsActive,
        //        IsAvailable = !slot.IsOccupied && slot.IsActive,
        //        NextAvailableTime = null
        //    };

        //    // If slot is occupied, try to find when it might become available
        //    if (slot.IsOccupied)
        //    {
        //        try
        //        {
        //            var activeSession = await _parkingSessionRepository.GetActiveSessionBySlotIdAsync(slot.Id);
        //            if (activeSession != null && activeSession.ExitTime.HasValue)
        //            {
        //                dto.NextAvailableTime = activeSession.ExitTime.Value;
        //            }
        //        }
        //        catch
        //        {
        //            // If we can't determine next available time, leave it null
        //        }
        //    }

        //    return dto;
        //}

        private async Task<ParkingSlotResponseDto> MapToParkingSlotResponseDto(ParkingSlot slot)
        {
            var dto = new ParkingSlotResponseDto
            {
                Id = slot.Id,
                SlotNumber = slot.SlotNumber,
                Floor = slot.Floor,
                Section = slot.Section,
                SlotImageBase64 = slot.SlotImage != null ? Convert.ToBase64String(slot.SlotImage) : null, // CHANGED THIS LINE
                IsOccupied = slot.IsOccupied,
                IsActive = slot.IsActive,
                IsAvailable = !slot.IsOccupied && slot.IsActive,
                NextAvailableTime = null
            };

            if (slot.IsOccupied)
            {
                try
                {
                    var activeSession = await _parkingSessionRepository.GetActiveSessionBySlotIdAsync(slot.Id);
                    if (activeSession != null && activeSession.ExitTime.HasValue)
                    {
                        dto.NextAvailableTime = activeSession.ExitTime.Value;
                    }
                }
                catch
                {
                    // If we can't determine next available time, leave it null
                }
            }

            return dto;
        }

        private byte[]? ConvertBase64ToByteArray(string? base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return null;

            try
            {
                // Remove data URL prefix if present (data:image/jpeg;base64,)
                if (base64String.Contains(","))
                {
                    base64String = base64String.Split(',')[1];
                }

                return Convert.FromBase64String(base64String);
            }
            catch
            {
                return null;
            }
        }
    }
}
