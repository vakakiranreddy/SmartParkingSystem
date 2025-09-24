using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);
        int? GetUserIdFromToken(string token);
        bool ValidateToken(string token);
    }
}
