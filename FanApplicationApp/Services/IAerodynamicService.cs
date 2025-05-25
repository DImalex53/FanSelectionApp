using SpeedCalc.Models;

namespace SpeedCalc.Services;

public interface IAerodynamicService
{
    public Task<byte[]> GenerateFileAsync(CalculationParameters parameters);
    public Task<byte[]> GeneratePngAsync(CalculationParameters parameters);
}
