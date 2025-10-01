using ScottPlot;
using ScottPlot.AxisPanels;
using SpeedCalc.Helpers.GetDiameterHelpers;
using SpeedCalc.Models;

namespace SpeedCalc.Helpers.PdfHelpers;
public class PaintDiagramsHelper
{
    private static readonly int PngWidth = 800;
    private static readonly int PngHeight = 600;

    public static byte[]? GenerateAerodynamicPng(List<AerodynamicsData> datas, SpeedCalculationParameters parameters)
    {
        var plot = GenerateAerodynamicPlot(datas, parameters);
        return plot.GetImageBytes(PngWidth, PngHeight);
    }

    public static Plot? GenerateAerodynamicPlot(List<AerodynamicsData> datas, SpeedCalculationParameters parameters)
    {
        int pointsCount = 100;

        var aerodynamicRow = AerodinamicRowHelper.GetAerodinamicRow(datas, parameters);

        var diameter = CalculationDiameterHelper.GetDiameter(datas, parameters);

        var (flowRates, staticPressures) = CalculationDiagramHelper.GetStaticPressureMassive(pointsCount, parameters, datas);

        var (_, totalPressures) = CalculationDiagramHelper.GetTotalPressureMassive(pointsCount, parameters, datas);

        var (_, powers) = CalculationDiagramHelper.GetPowerMassive(pointsCount, parameters, datas);
        var (flowRates1, pressureResistances) = CalculationDiagramHelper.GetPressureResistanceMassive(
            pointsCount, parameters);

        var totalPressureWorkPoint = CalculationDiagramHelper.GetPolinomTotalPressure(parameters.FlowRateRequired,
        datas, parameters);

        var totalEficiencyWorkPoint = CalculationDiagramHelper.GetPolinomEeficiency(parameters.FlowRateRequired,
        datas, parameters);

        var powerWorkPoint = CalculationDiagramHelper.GetPolinomPower(parameters.FlowRateRequired, datas, parameters);

        var staticPressureWorkPoint = CalculationDiagramHelper.GetPolinomStaticPressure(parameters.FlowRateRequired,
        parameters, datas);

        var staticEficiencyWorkPoint = CalculationDiagramHelper.GetPolinomStaticEficiency(parameters.FlowRateRequired,
          datas, parameters);

        var markImpeller = aerodynamicRow.NewMarkOfFan;
        if (parameters.SuctionType == 1)
        {
            markImpeller = aerodynamicRow.NewMarkOfFanD;
        }

        var aerodynamicPlot = new Plot();
        aerodynamicPlot.Title($"ТДМ {markImpeller}-{diameter * 10:F1} {parameters.Density} кг/м3 {parameters.Rpm} об/мин");
        aerodynamicPlot.XLabel("Расход воздуха, м³/ч");
        aerodynamicPlot.Axes.Left.Label.Text = "Давление, Па";
        aerodynamicPlot.Axes.Right.Label.Text = "Мощность, кВт";

        var staticPressurePlot = aerodynamicPlot.Add.Scatter(flowRates, staticPressures, Colors.DarkGrey);
        staticPressurePlot.LegendText = "Статическое давление";
        staticPressurePlot.MarkerSize = 0;
        staticPressurePlot.LinePattern = LinePattern.Dashed;

        var totalPressurePlot = aerodynamicPlot.Add.Scatter(flowRates, totalPressures, Colors.Black);
        totalPressurePlot.LegendText = "Полное давление";
        totalPressurePlot.MarkerSize = 0;
        totalPressurePlot.LinePattern = LinePattern.Solid;

        var resistancePlot = aerodynamicPlot.Add.Scatter(flowRates1, pressureResistances, Colors.Blue);
        resistancePlot.LegendText = "Характеристика сети";
        resistancePlot.MarkerSize = 0;
        resistancePlot.LinePattern = LinePattern.Solid;

        var powerPlot = aerodynamicPlot.Add.Scatter(flowRates, powers, Colors.Orange);
        powerPlot.LegendText = "Потребляемая мощность";
        powerPlot.Axes.YAxis = aerodynamicPlot.Axes.Right;
        powerPlot.MarkerSize = 0;
        powerPlot.LinePattern = LinePattern.Solid;

        double xMin = 0;
        double xMax = flowRates.Max() * 1.6; // Запас справа для информации
        double yMin = 0;
        double yMax = Math.Max(staticPressures.Max(), totalPressures.Max()) * 1.05;

        // ИНФОРМАЦИОННЫЙ БЛОК ПОД ЛЕГЕНДОЙ
        AddInfoBlockUnderLegend(
            aerodynamicPlot,
            parameters,
            staticPressureWorkPoint,
            totalPressureWorkPoint,
            xMax,
            yMax,
            totalPressureWorkPoint,
            powerWorkPoint,
            staticEficiencyWorkPoint,
            totalEficiencyWorkPoint);

        powerPlot.Axes.YAxis.Max = powers.Max() * 1.1;
        powerPlot.Axes.YAxis.Min = 0;
        aerodynamicPlot.Axes.SetLimits(xMin, xMax, yMin, yMax);

        aerodynamicPlot.ShowLegend();
        aerodynamicPlot.Legend.Alignment = Alignment.UpperRight;

        return aerodynamicPlot;
    }
    private static void AddInfoBlockUnderLegend(
        Plot plot,
        SpeedCalculationParameters parameters,
        double staticPressure,
        double totalPressure,
        double xMax,
        double yMax,
        double totalPressureWorkPoint,
        double powerWorkPoint,
        double staticEfficiencyWorkPoint,
        double totalEfficiencyWorkPoint)
    {
        // Создаем информационный блок
        string infoText = $"Заданная точка:\n" +
                         $"Расход: {parameters.FlowRateRequired:F1} м3/ч\n" +
                         $"Давление: {parameters.SystemResistance:F1} Па\n" +
                         $"Рабочая точка:\n" +
                         $"Статическое давление: {staticPressure:F1} Па\n" +
                         $"Статический КПД: {staticEfficiencyWorkPoint:F1} %\n" +
                         $"Полное давление: {totalPressure:F1} Па\n" +
                         $"Полный КПД: {totalEfficiencyWorkPoint:F1} %\n" +
                         $"Расход: {totalPressureWorkPoint:F1} м³/ч\n" +
                         $"Мощность: {powerWorkPoint:F1} кВт\n" +
                         $"Плотность на входе: {parameters.Density:F2} кг/м³";

        // Размещаем прямо под легендой с небольшим зазором
        // Такое же положение по X как у легенды (справа с отступом)
        double posX = xMax * 0.78; // Такое же положение как у легенды
        double posY = yMax * 0.5; // Прямо под легендой с небольшим зазором

        var text = plot.Add.Text(infoText, posX, posY);
        text.LabelFontColor = Colors.Black; // Черный цвет текста
        text.LabelFontSize = 14;
        text.Alignment = Alignment.UpperLeft;
        text.LabelBackgroundColor = Colors.White; // Белый фон
        text.LabelBorderColor = Colors.Black; // Черная рамка
        text.LabelPadding = 5;
    }

    public static Plot? GenerateTorquePlot(List<AerodynamicsData> datas, SpeedCalculationParameters parameters)
    {
        int pointsCount = 100;

        var (rpmValues, nominalTorques) = CalculationDiagramHelper.GetNominalTorqueMassive(pointsCount, datas, parameters);

        var (_, torqueWithGates) = CalculationDiagramHelper.GetTorqueWithGateMassive(pointsCount, datas, parameters);

        var torquePlot = new Plot();
        torquePlot.Title($"Нагрузочная характеристика электродвигателя");
        torquePlot.XLabel("Обороты рабочего колеса, об/мин");
        torquePlot.Axes.Left.Label.Text = "Момент силы, кН*м";

        var nominalTorquesPlot = torquePlot.Add.Scatter(rpmValues, nominalTorques, Colors.Grey);
        nominalTorquesPlot.LegendText = "Момент при открытой заслонке";

        var torqueWithGatesPlot = torquePlot.Add.Scatter(rpmValues, torqueWithGates, Colors.Black);
        torqueWithGatesPlot.LegendText = "Момент при закрытой заслонке на входе";

        torquePlot.ShowLegend();
        torquePlot.Legend.Alignment = Alignment.LowerRight;
        torqueWithGatesPlot.Axes.XAxis.Min = 0;
        nominalTorquesPlot.Axes.YAxis.Min = 0;

        return torquePlot;
    }
}




