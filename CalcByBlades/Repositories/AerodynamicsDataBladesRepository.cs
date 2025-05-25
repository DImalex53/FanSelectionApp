using BladesCalc.Repositories;
using BladesCalc.Models;
using Microsoft.EntityFrameworkCore;
using BladesCalc.Data;

namespace BladesCalc.Repositories;

public class AerodynamicsDataBladesRepository : IAerodynamicsDataBladesRepository
{
    private readonly AerodynamicsDataBladesContext _context;

    public AerodynamicsDataBladesRepository(AerodynamicsDataBladesContext context)
    {
        _context = context;
    }

    public async Task<AerodynamicsDataBlades> GetByIdAsync(Guid id)
    {
        return await _context.AerodynamicsDataBlades.FindAsync(id);
    }

    public async Task<IEnumerable<AerodynamicsDataBlades>> GetAllAsync()
    {
        return await _context.AerodynamicsDataBlades.ToListAsync();
    }

    public async Task AddAsync(AerodynamicsDataBlades data)
    {
        await _context.AerodynamicsDataBlades.AddAsync(data);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AerodynamicsDataBlades data)
    {
        _context.AerodynamicsDataBlades.Update(data);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var data = await GetByIdAsync(id);
        if (data != null)
        {
            _context.AerodynamicsDataBlades.Remove(data);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.AerodynamicsDataBlades.AnyAsync(e => e.Id == id);
    }
}