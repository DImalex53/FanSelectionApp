using BladesCalc.Helpers.AerodynamicHelpers;
using BladesCalc.Helpers.PdfHelpers;
using BladesCalc.Models;
using BladesCalc.Repositories;
using Common;

namespace BladesCalc.Services;

public class AerodynamicService(IAerodynamicsDataBladesRepository aerodynamicsDataRepository) : IAerodynamicService
{
    private readonly IAerodynamicsDataBladesRepository _aerodynamicsDataRepository = aerodynamicsDataRepository;

    public async Task<List<GraphData>> GetAllGraphsAsync(BladesCalculationParameters parameters)
    {
        var allData = (await _aerodynamicsDataRepository.GetAllAsync()).ToList();
        var graphs = new List<GraphData>();

        // Получаем все возможные варианты по типу лопаток
        var bladeTypes = new[] { 1, 2, 3, 4, 5 }; // Все типы лопаток
        var pressureTypes = new[] { 0, 1 }; // По статическому и полному давлению

        int graphId = 1;
        foreach (var bladeType in bladeTypes)
        {
            foreach (var pressureType in pressureTypes)
            {
                try
                {
                    // Создаем копию параметров с текущими типами
                    var currentParams = new BladesCalculationParameters
                    {
                        FlowRateRequired = parameters.FlowRateRequired,
                        SystemResistance = parameters.SystemResistance,
                        Density = parameters.Density,
                        SuctionType = parameters.SuctionType,
                        TypeOfBladesKod = bladeType,
                        TypeOfPressure = pressureType,
                        RightSchemeChoose = $"Автоматический подбор - Тип лопаток {bladeType}, Давление {(pressureType == 0 ? "Статическое" : "Полное")}",
                        NalichieVFD = parameters.NalichieVFD,
                        TypeOfChoose = parameters.TypeOfChoose
                    };

                    var aerodinamicByTypeBladesRow = AerodinamicHelper.GetAerodynamicByTypeBladesRow(allData, currentParams);
                    
                    if (aerodinamicByTypeBladesRow != null)
                    {
                        // Получаем данные о схеме для диаметра и оборотов
                        var rowOfRightSchemes = AerodinamicHelper.GetRowOfRightSchemes(allData, currentParams, new ParametersDrawImage());
                        
                        if (rowOfRightSchemes != null)
                        {
                            var aerodynamicPlot = PaintDiagramHelper.GetDiagrameDraw(
                                currentParams,
                                aerodinamicByTypeBladesRow.StaticPressure1,
                                aerodinamicByTypeBladesRow.StaticPressure2,
                                aerodinamicByTypeBladesRow.StaticPressure3,
                                aerodinamicByTypeBladesRow.MinDeltaEfficiency,
                                aerodinamicByTypeBladesRow.MaxDeltaEfficiency,
                                aerodinamicByTypeBladesRow.OutletLength,
                                aerodinamicByTypeBladesRow.OutletWidth,
                                aerodinamicByTypeBladesRow.Efficiency1,
                                aerodinamicByTypeBladesRow.Efficiency2,
                                aerodinamicByTypeBladesRow.Efficiency3,
                                aerodinamicByTypeBladesRow.Efficiency4,
                                aerodinamicByTypeBladesRow.NewMarkOfFan,
                                aerodinamicByTypeBladesRow.NewMarkOfFand,
                                rowOfRightSchemes.Diameter,
                                rowOfRightSchemes.Rpm);

                            if (aerodynamicPlot != null)
                            {
                                // Конвертируем график в изображение
                                var imageBytes = PaintDiagramHelper.GetDiagramAsImageBytes(
                                    currentParams,
                                    aerodinamicByTypeBladesRow.StaticPressure1,
                                    aerodinamicByTypeBladesRow.StaticPressure2,
                                    aerodinamicByTypeBladesRow.StaticPressure3,
                                    aerodinamicByTypeBladesRow.MinDeltaEfficiency,
                                    aerodinamicByTypeBladesRow.MaxDeltaEfficiency,
                                    aerodinamicByTypeBladesRow.OutletLength,
                                    aerodinamicByTypeBladesRow.OutletWidth,
                                    aerodinamicByTypeBladesRow.Efficiency1,
                                    aerodinamicByTypeBladesRow.Efficiency2,
                                    aerodinamicByTypeBladesRow.Efficiency3,
                                    aerodinamicByTypeBladesRow.Efficiency4,
                                    aerodinamicByTypeBladesRow.NewMarkOfFan,
                                    aerodinamicByTypeBladesRow.NewMarkOfFand,
                                    rowOfRightSchemes.Diameter,
                                    rowOfRightSchemes.Rpm,
                                    800, 600, "PNG");

                                var graphData = new GraphData
                                {
                                    GraphId = graphId++,
                                    GraphName = $"Тип лопаток: {GetBladeTypeName(bladeType)}, Давление: {(pressureType == 0 ? "Статическое" : "Полное")}",
                                    GraphImage = imageBytes,
                                    Efficiency = aerodinamicByTypeBladesRow.Efficiency1,
                                    StaticPressure = aerodinamicByTypeBladesRow.StaticPressure1,
                                    Diameter = rowOfRightSchemes.Diameter,
                                    Rpm = rowOfRightSchemes.Rpm,
                                    FanMark = aerodinamicByTypeBladesRow.NewMarkOfFan ?? "",
                                    FanMarkD = aerodinamicByTypeBladesRow.NewMarkOfFand ?? "",
                                    ImpellerWidth = aerodinamicByTypeBladesRow.ImpellerWidth,
                                    BladeWidth = aerodinamicByTypeBladesRow.BladeWidth,
                                    BladeLength = aerodinamicByTypeBladesRow.BladeLength,
                                    NumberOfBlades = aerodinamicByTypeBladesRow.NumberOfBlades,
                                    ImpellerInletDiameter = aerodinamicByTypeBladesRow.ImpellerInletDiameter,
                                    TypeOfBlades = GetBladeTypeName(bladeType)
                                };

                                graphs.Add(graphData);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Логируем ошибку, но продолжаем с другими вариантами
                    Console.WriteLine($"Ошибка при генерации графика для типа лопаток {bladeType}, давления {pressureType}: {ex.Message}");
                }
            }
        }

        return graphs;
    }

    public async Task<byte[]> GenerateFileAsync(BladesCalculationParameters parameters)
    {
        var allData = (await _aerodynamicsDataRepository.GetAllAsync()).ToList();

        var aerodinamicByTypeBladesRow = AerodinamicHelper.GetAerodynamicByTypeBladesRow(allData, parameters);
        var rowOfRightSchemes = AerodinamicHelper.GetRowOfRightSchemes(allData, parameters, new ParametersDrawImage());

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
        var diameter = rowOfRightSchemes.Diameter;
        var rpm = rowOfRightSchemes.Rpm;
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

    private static string GetBladeTypeName(int bladeType) => bladeType switch
    {
        1 => "Назад загнутые",
        2 => "Радиально оканчивающиеся",
        3 => "Аэрофольные",
        4 => "Прямые, отклоенные назад",
        5 => "Вперед загнутые",
        _ => "Неизвестный тип"
    };
}
