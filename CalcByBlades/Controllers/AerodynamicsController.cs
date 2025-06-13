using Microsoft.AspNetCore.Mvc;
using BladesCalc.Models;
using BladesCalc.Services;

namespace BladesCalc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AerodynamicsController(IAerodynamicService calculationService) : ControllerBase
{
    private readonly IAerodynamicService _service = calculationService;

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CalculationParameters parameters, ParametersDrawImage parametersDrawImage)
    {
        if (parameters == null)
            return BadRequest("Параметры не могут быть пустыми");

        if (parameters.FlowRateRequired <= 0)
            return BadRequest("Расход должен быть положительным числом");

        await _service.DownloadFileAsync(parameters, parametersDrawImage);

        return Ok();
    }
}