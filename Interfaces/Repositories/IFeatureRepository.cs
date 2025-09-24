using SmartParkingSystem.Models;

namespace SmartParkingSystem.Interfaces.Repositories
{
    public interface IFeatureRepository : IBaseRepository<Feature>
    {
        Task<IEnumerable<Feature>> GetActiveFeaturesAsync();
        Task<Feature> GetByNameAsync(string name);
        Task<bool> NameExistsAsync(string name);
    }
}
