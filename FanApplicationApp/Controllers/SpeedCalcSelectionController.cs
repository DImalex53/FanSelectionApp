using Microsoft.AspNetCore.Mvc;
using SpeedCalc.Models;
using SpeedCalc.Services;

namespace SpeedCalc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpeedCalcSelectionController(IAerodynamicService calculationService) : ControllerBase
{
    private readonly IAerodynamicService _service = calculationService;

    [HttpPost("downloadfile")]
    public async Task<IActionResult> DownloadFile([FromBody] CalculationParameters parameters)
    {
        if (parameters == null)
            return BadRequest("Параметры не могут быть пустыми");

        if (parameters.FlowRateRequired <= 0)
            return BadRequest("Расход должен быть положительным числом");

        var fileContent = await _service.GenerateFileAsync(parameters);

        return File(fileContent, "application/pdf", "SpeedCalc.pdf");
    }

    [HttpPost("downloadpng")]
    public async Task<IActionResult> DownloadPng([FromBody] CalculationParameters parameters)
    {
        if (parameters == null)
            return BadRequest("Параметры не могут быть пустыми");

        if (parameters.FlowRateRequired <= 0)
            return BadRequest("Расход должен быть положительным числом");

        var fileContent = await _service.GeneratePngAsync(parameters);

        return File(fileContent, "image/png", "AerodynamicPlot.png");
    }
}