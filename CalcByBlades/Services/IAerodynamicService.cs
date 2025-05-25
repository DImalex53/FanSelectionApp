using BladesCalc.Models;

namespace BladesCalc.Services;

public interface IAerodynamicService
{
    public Task DownloadFileAsync(CalculationParameters parameters, ParametersDrawImage parametersDrawImage);
}
