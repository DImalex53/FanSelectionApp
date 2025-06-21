using SpeedCalc.Models;

namespace SpeedCalc.Services;

public interface IAerodynamicService
{
    public Task<byte[]> GenerateFileAsync(SpeedCalculationParameters parameters);
    public Task<byte[]> GeneratePngAsync(SpeedCalculationParameters parameters);
}
