using SmartParkingSystem.DTOs.BroadcastNotification;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Services
{
    public interface IBroadcastNotificationService
    {
        // Basic CRUD
        Task<BroadcastNotificationResponseDto> GetByIdAsync(int id);
        Task<IEnumerable<BroadcastNotificationResponseDto>> GetAllAsync();
        Task<BroadcastNotificationResponseDto> CreateAsync(CreateBroadcastNotificationDto createDto);
        Task<BroadcastNotificationResponseDto> UpdateAsync(int id, UpdateBroadcastNotificationDto updateDto);
        Task<bool> DeleteAsync(int id);

        // Broadcast-specific methods
        Task<bool> SendBroadcastAsync(int broadcastId);
        Task<IEnumerable<BroadcastNotificationResponseDto>> GetByTargetRoleAsync(UserRole? targetRole);
        Task<bool> ProcessPendingBroadcastsAsync();
    }

}
