using BladesCalc.Models;
using Common;

namespace BladesCalc.Services;

public interface IAerodynamicService
{
    public Task DownloadFileAsync(BladesCalculationParameters parameters, ParametersDrawImage parametersDrawImage);
}
