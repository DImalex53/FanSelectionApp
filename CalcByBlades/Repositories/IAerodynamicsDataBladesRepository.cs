using BladesCalc.Models;

namespace BladesCalc.Repositories;

public interface IAerodynamicsDataBladesRepository
{
    Task<AerodynamicsDataBlades> GetByIdAsync(Guid id);
    Task<IEnumerable<AerodynamicsDataBlades>> GetAllAsync();
    Task AddAsync(AerodynamicsDataBlades data);
    Task UpdateAsync(AerodynamicsDataBlades data);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
