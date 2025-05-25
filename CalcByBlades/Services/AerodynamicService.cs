using BladesCalc.Helpers.PdfHelpers;
using BladesCalc.Models;
using BladesCalc.Repositories;
using ScottPlot;
using System.Runtime.Intrinsics.Arm;

namespace BladesCalc.Services;

public class AerodynamicService(IAerodynamicsDataBladesRepository aerodynamicsDataRepository) : IAerodynamicService
{
    private readonly IAerodynamicsDataBladesRepository _aerodynamicsDataRepository = aerodynamicsDataRepository;

    public async Task DownloadFileAsync(CalculationParameters parameters, ParametersDrawImage parametersDrawImage)
    {
        var allData = (await _aerodynamicsDataRepository.GetAllAsync()).ToList();

        var tableOfRightSchemes = PaintDiagramsHelper.GenerateTableOfRightSchemes(allData, parameters, parametersDrawImage);

        if (tableOfRightSchemes == null) return;

        var aerodynamicPlot = PaintDiagramHelper.GetDiagrameDraw(
            parameters,
            staticPressure1,
            staticPressure2,
            staticPressure3,
            minDeltaEfficiency,
            maxDeltaEfficiency,
            outletLength,
            outletWidth,
            efficiency1,
            efficiency2,
            efficiency3,
            efficiency4,
            newMarkOfFan,
            newMarkOfFand,
            diameter,
            rpm);
        var torquePlot = PaintDiagramHelper.GenerateTorquePlot(
            parameters,
            staticPressure1,
            staticPressure2,
            staticPressure3,
            minDeltaEfficiency,
            maxDeltaEfficiency,
            outletLength,
            outletWidth,
            efficiency1,
            efficiency2,
            efficiency3,
            efficiency4,
            newMarkOfFan,
            newMarkOfFand,
            diameter,
            rpm);

        if (aerodynamicPlot == null) return;

        string reportPath = Path.Combine("wwwroot", "reports", "report.pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);

        PdfExporter.ExportToPdf(
            aerodynamicPlot,
            torquePlot,
            pdfPath,
            parameters,
            staticPressure1,
            staticPressure2,
            staticPressure3,
            minDeltaEfficiency,
            maxDeltaEfficiency,
            outletLength,
            outletWidth,
            efficiency1,
            efficiency2,
            efficiency3,
            efficiency4,
            newMarkOfFan,
            newMarkOfFand,
            diameter,
            impellerWidth,
            bladeWidth,
            bladeLength,
            numberOfBlades,
            impellerInletDiameter,
            typeOfBlades,
            new PdfExportOptions
            {
                Title = $"Отчет о подборе по задаче {parameters.NumberOfTask}",
                Orientation = PdfSharp.PageOrientation.Landscape,
                FontFamily = "Times New Roman"
            });
    }
}
