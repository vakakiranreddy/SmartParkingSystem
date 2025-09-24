using SmartParkingSystem.DTOs.SlotFeature;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Interfaces.Services;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Services
{
    public class SlotFeatureService : ISlotFeatureService
    {
        private readonly ISlotFeatureRepository _slotFeatureRepository;
        private readonly IParkingSlotRepository _slotRepository;
        private readonly IFeatureRepository _featureRepository;

        public SlotFeatureService(
            ISlotFeatureRepository slotFeatureRepository,
            IParkingSlotRepository slotRepository,
            IFeatureRepository featureRepository)
        {
            _slotFeatureRepository = slotFeatureRepository;
            _slotRepository = slotRepository;
            _featureRepository = featureRepository;
        }

        public async Task<SlotFeatureResponseDto> AssignFeatureToSlotAsync(AssignSlotFeatureDto assignDto)
        {
            if (await _slotFeatureRepository.SlotFeatureExistsAsync(assignDto.SlotId, assignDto.FeatureId))
                throw new InvalidOperationException("This feature is already assigned to the slot.");

            var slot = await _slotRepository.GetByIdAsync(assignDto.SlotId)
                       ?? throw new ArgumentException($"Slot {assignDto.SlotId} not found.");

            var feature = await _featureRepository.GetByIdAsync(assignDto.FeatureId)
                          ?? throw new ArgumentException($"Feature {assignDto.FeatureId} not found.");

            var slotFeature = new SlotFeature
            {
                SlotId = slot.Id,
                FeatureId = feature.Id,
                IsActive = true
            };

            var created = await _slotFeatureRepository.AddAsync(slotFeature);

            return MapToDto(created, slot.SlotNumber, feature.Name, feature.PriceModifier);
        }

        public async Task<bool> RemoveFeatureFromSlotAsync(RemoveSlotFeatureDto removeDto)
        {
            var slotFeature = await _slotFeatureRepository.GetSlotFeatureAsync(removeDto.SlotId, removeDto.FeatureId);
            if (slotFeature == null)
                throw new ArgumentException("Feature not assigned to this slot.");

            return await _slotFeatureRepository.DeleteAsync(removeDto.SlotId, removeDto.FeatureId);
        }


        public async Task<IEnumerable<SlotFeatureResponseDto>> GetSlotFeaturesAsync(int slotId)
        {
            var slot = await _slotRepository.GetByIdAsync(slotId)
                       ?? throw new ArgumentException($"Slot {slotId} not found.");

            var slotFeatures = await _slotFeatureRepository.GetBySlotIdAsync(slotId);
            var result = new List<SlotFeatureResponseDto>();

            foreach (var sf in slotFeatures)
            {
                var feature = await _featureRepository.GetByIdAsync(sf.FeatureId);
                if (feature != null)
                    result.Add(MapToDto(sf, slot.SlotNumber, feature.Name, feature.PriceModifier));
            }

            return result;
        }

        public async Task<IEnumerable<SlotFeatureResponseDto>> GetFeatureAssignmentsAsync(int featureId)
        {
            var feature = await _featureRepository.GetByIdAsync(featureId)
                          ?? throw new ArgumentException($"Feature {featureId} not found.");

            var assignments = await _slotFeatureRepository.GetByFeatureIdAsync(featureId);
            var result = new List<SlotFeatureResponseDto>();

            foreach (var sf in assignments)
            {
                var slot = await _slotRepository.GetByIdAsync(sf.SlotId);
                if (slot != null)
                    result.Add(MapToDto(sf, slot.SlotNumber, feature.Name, feature.PriceModifier));
            }

            return result;
        }

        private SlotFeatureResponseDto MapToDto(SlotFeature sf, string slotNumber, string featureName, decimal priceModifier)
        {
            return new SlotFeatureResponseDto
            {
                SlotId = sf.SlotId,
                SlotNumber = slotNumber,
                FeatureId = sf.FeatureId,
                FeatureName = featureName,
                PriceModifier = priceModifier,
                IsActive = sf.IsActive
            };
        }
    }
}
