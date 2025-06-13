using BladesCalc.Helpers.AerodynamicHelpers;
using BladesCalc.Helpers.PdfHelpers;
using BladesCalc.Models;
using BladesCalc.Repositories;

namespace BladesCalc.Services;

public class AerodynamicService(IAerodynamicsDataBladesRepository aerodynamicsDataRepository) : IAerodynamicService
{
    private readonly IAerodynamicsDataBladesRepository _aerodynamicsDataRepository = aerodynamicsDataRepository;

    public async Task DownloadFileAsync(CalculationParameters parameters, ParametersDrawImage parametersDrawImage)
    {
        var allData = (await _aerodynamicsDataRepository.GetAllAsync()).ToList();

        var aerodinamicByTypeBladesRow = AerodinamicHelper.GetAerodynamicByTypeBladesRow (allData, parameters);
        var rowOfRightSchemes = AerodinamicHelper.GetRowOfRightSchemes(allData, parameters, parametersDrawImage);

        var staticPressure1 = aerodinamicByTypeBladesRow.StaticPressure1;
        var staticPressure2 = aerodinamicByTypeBladesRow.StaticPressure2;
        var staticPressure3 = aerodinamicByTypeBladesRow.StaticPressure3;
        var minDeltaEfficiency = aerodinamicByTypeBladesRow.MinDeltaEfficiency;
        var maxDeltaEfficiency = aerodinamicByTypeBladesRow.MinDeltaEfficiency;
        var outletLength = aerodinamicByTypeBladesRow.OutletLength;
        var outletWidth = aerodinamicByTypeBladesRow.OutletWidth;
        var efficiency1 = aerodinamicByTypeBladesRow.Efficiency1;
        var efficiency2 = aerodinamicByTypeBladesRow.Efficiency2;
        var efficiency3 = aerodinamicByTypeBladesRow.Efficiency3;
        var efficiency4 = aerodinamicByTypeBladesRow.Efficiency4;
        var newMarkOfFan = aerodinamicByTypeBladesRow.NewMarkOfFan;
        var newMarkOfFand = aerodinamicByTypeBladesRow.NewMarkOfFand;
        var diameter = rowOfRightSchemes.Diameter;
        var rpm = rowOfRightSchemes.Rpm;
        var impellerWidth = aerodinamicByTypeBladesRow.ImpellerWidth;
        var bladeWidth = aerodinamicByTypeBladesRow.BladeWidth;
        var bladeLength = aerodinamicByTypeBladesRow.BladeLength;
        var numberOfBlades = aerodinamicByTypeBladesRow.NumberOfBlades;
        var impellerInletDiameter = aerodinamicByTypeBladesRow.ImpellerInletDiameter;
        var typeOfBlades = aerodinamicByTypeBladesRow.TypeOfBlades;

        if (aerodinamicByTypeBladesRow == null) return;

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

        string pdfPath = Path.Combine("wwwroot", "reports", "report.pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(pdfPath)!);

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
