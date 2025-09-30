using Microsoft.AspNetCore.Mvc;
using BladesCalc.Models;
using BladesCalc.Services;

namespace BladesCalc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AerodynamicsController(IAerodynamicService calculationService) : ControllerBase
{
    private readonly IAerodynamicService _service = calculationService;

    [HttpPost("get-graphs")]
    public async Task<IActionResult> GetGraphs([FromBody] BladesCalculationParameters parameters)
    {
        if (parameters == null)
            return BadRequest("Параметры не могут быть пустыми");

        if (parameters.FlowRateRequired <= 0)
            return BadRequest("Расход должен быть положительным числом");

        try
        {
            var graphsData = await _service.GetAllGraphsAsync(parameters);
            return Ok(graphsData);
        }
        catch (Exception ex)
        {
            return BadRequest($"Ошибка при получении графиков: {ex.Message}");
        }
    }

    [HttpPost("download-file")]
    public async Task<IActionResult> DownloadFile([FromBody] BladesCalculationParameters parameters)
    {
        if (parameters == null)
            return BadRequest("Параметры не могут быть пустыми");

        try
        {
            // Убрал параметр drawParameters, так как в сервисе он не используется
            var fileBytes = await _service.GenerateFileAsync(parameters);
            return File(fileBytes, "application/pdf", $"Техническое_предложение_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest($"Ошибка при генерации файла: {ex.Message}");
        }
    }

    [HttpGet("get-all-schemes")]
    public async Task<IActionResult> GetAllSchemes()
    {
        try
        {
            var schemes = await _service.GetAllSchemesAsync();
            return Ok(schemes);
        }
        catch (Exception ex)
        {
            return BadRequest($"Ошибка при получении схем: {ex.Message}");
        }
    }
}