using SmartParkingSystem.DTOs.Feature;
using SmartParkingSystem.DTOs.ParkingRate;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Interfaces.Services;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Services
{
    public class FeatureService : IFeatureService
    {
        private readonly IFeatureRepository _featureRepository;

        public FeatureService(IFeatureRepository featureRepository)
        {
            _featureRepository = featureRepository;
        }

        public async Task<IEnumerable<FeatureResponseDto>> GetAllAsync()
        {
            var features = await _featureRepository.GetAllAsync();
            return features.Select(MapToDto);
        }

        public async Task<FeatureResponseDto> GetByIdAsync(int id)
        {
            var feature = await _featureRepository.GetByIdAsync(id);
            if (feature == null)
                throw new ArgumentException($"Feature with Id {id} not found.");

            return MapToDto(feature);
        }

        public async Task<FeatureResponseDto> CreateAsync(CreateFeatureDto createDto)
        {
            if (await _featureRepository.NameExistsAsync(createDto.Name))
                throw new InvalidOperationException($"Feature name '{createDto.Name}' already exists.");

            var feature = new Feature
            {
                Name = createDto.Name,
                Description = createDto.Description,
                IconUrl = createDto.IconUrl,
                PriceModifier = createDto.PriceModifier,
                IsActive = true
            };

            var created = await _featureRepository.AddAsync(feature);
            return MapToDto(created);
        }

        public async Task<FeatureResponseDto> UpdateAsync(int id, UpdateFeatureDto updateDto)
        {
            var feature = await _featureRepository.GetByIdAsync(id);
            if (feature == null)
                throw new ArgumentException($"Feature with Id {id} not found.");

            // check duplicate
            var existing = await _featureRepository.GetByNameAsync(updateDto.Name);
            if (existing != null && existing.Id != id)
                throw new InvalidOperationException($"Feature name '{updateDto.Name}' already exists.");

            feature.Name = updateDto.Name;
            feature.Description = updateDto.Description;
            feature.IconUrl = updateDto.IconUrl;
            feature.PriceModifier = updateDto.PriceModifier;
            feature.IsActive = updateDto.IsActive;

            var updated = await _featureRepository.UpdateAsync(feature);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var feature = await _featureRepository.GetByIdAsync(id);
            if (feature == null)
                throw new ArgumentException($"Feature with Id {id} not found.");

            return await _featureRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<FeatureResponseDto>> GetActiveFeaturesAsync()
        {
            var features = await _featureRepository.GetActiveFeaturesAsync();
            return features.Select(MapToDto);
        }

        private FeatureResponseDto MapToDto(Feature f) => new FeatureResponseDto
        {
            Id = f.Id,
            Name = f.Name,
            Description = f.Description,
            IconUrl = f.IconUrl,
            PriceModifier = f.PriceModifier,
            IsActive = f.IsActive
        };
    }
}
