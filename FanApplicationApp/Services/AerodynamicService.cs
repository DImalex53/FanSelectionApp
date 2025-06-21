using SpeedCalc.Helpers.PdfHelpers;
using SpeedCalc.Models;
using SpeedCalc.Repositories;

namespace SpeedCalc.Services;

public class AerodynamicService(IAerodynamicsDataRepository aerodynamicsDataRepository) : IAerodynamicService
{
    private readonly IAerodynamicsDataRepository _aerodynamicsDataRepository = aerodynamicsDataRepository;


    public async Task<byte[]> GenerateFileAsync(SpeedCalculationParameters parameters)
    {
        var allData = (await _aerodynamicsDataRepository.GetAllAsync()).ToList();

        var aerodynamicPlot = PaintDiagramsHelper.GenerateAerodynamicPlot(allData, parameters);
        var torquePlot = PaintDiagramsHelper.GenerateTorquePlot(allData, parameters);

        if (aerodynamicPlot == null)
        {
            throw new InvalidOperationException("Не удалось сгенерировать аэродинамический график");
        }

        // Создаем PDF в памяти
        var document = PdfExporter.CreatePdfDocument(
            allData.ToList(),
            aerodynamicPlot,
            torquePlot,
            parameters,
            new PdfExportOptions
            {
                Title = $"Отчет о подборе по задаче {parameters.NumberOfTask}",
                Orientation = PdfSharp.PageOrientation.Landscape,
                FontFamily = "Times New Roman"
            });

        return document;
    }

    public async Task<byte[]> GeneratePngAsync(SpeedCalculationParameters parameters)
    {
        var allData = (await _aerodynamicsDataRepository.GetAllAsync()).ToList();
        var png = PaintDiagramsHelper.GenerateAerodynamicPng(allData, parameters);

        return png;
    }
}