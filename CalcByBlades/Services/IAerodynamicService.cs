using BladesCalc.Models;
using Common;

namespace BladesCalc.Services;

public interface IAerodynamicService
{
    public Task<List<GraphData>> GetAllGraphsAsync(BladesCalculationParameters parameters);
    public Task<byte[]> GenerateFileAsync(BladesCalculationParameters parameters);
}
