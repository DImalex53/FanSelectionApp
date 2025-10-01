using BladesCalc.Helpers.AerodynamicHelpers;
using BladesCalc.Helpers.PdfHelpers;
using BladesCalc.Models;
using BladesCalc.Repositories;
using Common;

namespace BladesCalc.Services;

public class AerodynamicService(IAerodynamicsDataBladesRepository aerodynamicsDataRepository) : IAerodynamicService
{
    private readonly IAerodynamicsDataBladesRepository _dataBladesRepository = aerodynamicsDataRepository;

    public async Task<List<DatasRightVents>> GetAllGraphsAsync(BladesCalculationParameters parameters)
    {
        var allData = (await _dataBladesRepository.GetAllAsync()).ToList();

        var aerodinamicsByTypeBlades = AerodinamicHelper.GetAerodynamicByTypeBlades(allData, parameters);

        return PaintDiagramsHelper.GenerateTableOfRightVents(aerodinamicsByTypeBlades, parameters, new ParametersDrawImage());
    }

    public async Task<byte[]> GenerateFileAsync(BladesCalculationParameters parameters)
    {
        var allData = (await _dataBladesRepository.GetAllAsync()).ToList();
        var rowVent = AerodinamicHelper.GetRowOfRightVent(allData, parameters, new ParametersDrawImage());
        var aerodinamicByTypeBladesRow = AerodinamicHelper.GetAerodynamicByTypeBladesRow(allData, parameters, new ParametersDrawImage(), rowVent);

        if (aerodinamicByTypeBladesRow == null)
            throw new InvalidOperationException("Не удалось получить данные для выбранного варианта");

        var staticPressure1 = aerodinamicByTypeBladesRow.StaticPressure1;
        var staticPressure2 = aerodinamicByTypeBladesRow.StaticPressure2;
        var staticPressure3 = aerodinamicByTypeBladesRow.StaticPressure3;
        var minDeltaEfficiency = aerodinamicByTypeBladesRow.MinDeltaEfficiency;
        var maxDeltaEfficiency = aerodinamicByTypeBladesRow.MaxDeltaEfficiency;
        var outletLength = aerodinamicByTypeBladesRow.OutletLength;
        var outletWidth = aerodinamicByTypeBladesRow.OutletWidth;
        var efficiency1 = aerodinamicByTypeBladesRow.Efficiency1;
        var efficiency2 = aerodinamicByTypeBladesRow.Efficiency2;
        var efficiency3 = aerodinamicByTypeBladesRow.Efficiency3;
        var efficiency4 = aerodinamicByTypeBladesRow.Efficiency4;
        var newMarkOfFan = aerodinamicByTypeBladesRow.NewMarkOfFan;
        var newMarkOfFand = aerodinamicByTypeBladesRow.NewMarkOfFand;
        var diameter = rowVent.Diameter;
        var rpm = rowVent.Rpm;
        var impellerWidth = aerodinamicByTypeBladesRow.ImpellerWidth;
        var bladeWidth = aerodinamicByTypeBladesRow.BladeWidth;
        var bladeLength = aerodinamicByTypeBladesRow.BladeLength;
        var numberOfBlades = aerodinamicByTypeBladesRow.NumberOfBlades;
        var impellerInletDiameter = aerodinamicByTypeBladesRow.ImpellerInletDiameter;
        var typeOfBlades = aerodinamicByTypeBladesRow.TypeOfBlades;

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

        if (aerodynamicPlot == null)
            throw new InvalidOperationException("Не удалось сгенерировать аэродинамический график");

        // Создаем PDF в памяти
        string tempPdfPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pdf");

        PdfExporter.ExportToPdf(
            aerodynamicPlot,
            torquePlot,
            tempPdfPath,
            allData,
            parameters,
            new ParametersDrawImage(),
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
            newMarkOfFan ?? "",
            newMarkOfFand ?? "",
            diameter,
            impellerWidth,
            bladeWidth,
            bladeLength,
            numberOfBlades,
            impellerInletDiameter,
            typeOfBlades ?? "",
            new PdfExportOptions
            {
                Title = $"Отчет о подборе по задаче {parameters.NumberOfTask}",
                Orientation = PdfSharp.PageOrientation.Landscape,
                FontFamily = "Times New Roman"
            });

        // Читаем созданный файл и возвращаем его содержимое
        var fileBytes = await File.ReadAllBytesAsync(tempPdfPath);

        // Удаляем временный файл
        if (File.Exists(tempPdfPath))
            File.Delete(tempPdfPath);

        return fileBytes;
    }

    public async Task<List<string>> GetAllSchemesAsync()
    {
        var allData = (await _dataBladesRepository.GetAllAsync()).ToList();

        return allData.Select(x => x.Scheme).ToList();
    }
}
