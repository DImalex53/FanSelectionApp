using iText.Layout.Renderer;
using ScottPlot;
using ScottPlot.AxisPanels;
using SpeedCalc.Helpers.GetDiameterHelpers;
using SpeedCalc.Models;

namespace SpeedCalc.Helpers.PdfHelpers;
public class PaintDiagramsHelper
{

    private const int pointsCount = 100;

    public static byte[]? GenerateAerodynamicPng(List<AerodynamicsData> datas, SpeedCalculationParameters parameters,ParametersDrawImage parametersDrawImage)
    {
        var plot = GenerateAerodynamicPlot(datas, parameters);
        return plot.GetImageBytes(parametersDrawImage.Width, parametersDrawImage.Height);
    }

    public static Plot? GenerateAerodynamicPlot(List<AerodynamicsData> datas, SpeedCalculationParameters parameters)
    {
        int pointsCount = 100;

        var aerodynamicRow = AerodinamicRowHelper.GetAerodinamicRow(datas, parameters);

        var diameter = CalculationDiameterHelper.GetDiameter(datas, parameters);

        var rpm = parameters.Rpm;

        var workPoint = PaintDiagramsHelper.FindIntersectionPresurePoint(parameters, datas, rpm);

        if (parameters.NalichieVFD == true)
        {
            rpm = PaintDiagramsHelper.GetRpmVFD(parameters.SystemResistance,rpm,workPoint.pressure);
        }

        var (flowRates, staticPressures) = CalculationDiagramHelper.GetStaticPressureMassive(pointsCount, parameters, datas, rpm);

        var (_, totalPressures) = CalculationDiagramHelper.GetTotalPressureMassive(pointsCount, parameters, datas, rpm);

        var (_, powers) = CalculationDiagramHelper.GetPowerMassive(pointsCount, parameters, datas, rpm);
        var (flowRates1, pressureResistances) = CalculationDiagramHelper.GetPressureResistanceMassive(
            pointsCount, parameters, workPoint.flowRate);

        var totalPressureWorkPoint = CalculationDiagramHelper.GetPolinomTotalPressure(workPoint.flowRate,
        datas, parameters, rpm);

        var totalEficiencyWorkPoint = CalculationDiagramHelper.GetPolinomEeficiency(workPoint.flowRate,
        datas, parameters, rpm);

        var powerWorkPoint = CalculationDiagramHelper.GetPolinomPower(workPoint.flowRate, datas, parameters, rpm);

        var staticPressureWorkPoint = workPoint.pressure;

        var staticEficiencyWorkPoint = CalculationDiagramHelper.GetPolinomStaticEficiency(workPoint.flowRate,
          datas, parameters, rpm);

        var markImpeller = aerodynamicRow.NewMarkOfFan;
        if (parameters.SuctionType == 1)
        {
            markImpeller = aerodynamicRow.NewMarkOfFanD;
        }

        var aerodynamicPlot = new Plot();
        aerodynamicPlot.Title($"ТДМ {markImpeller}-{diameter * 10:F1}_{rpm} об/мин_{parameters.Density} кг/м3");
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

        aerodynamicPlot.Add.Marker(
               workPoint.flowRate,
               totalPressureWorkPoint, MarkerShape.FilledCircle, 5, Colors.Red);
        aerodynamicPlot.Add.Marker(
                workPoint.flowRate,
                staticPressureWorkPoint, MarkerShape.FilledCircle, 5, Colors.Red);
        aerodynamicPlot.Add.Marker(
                parameters.FlowRateRequired,
                parameters.SystemResistance, MarkerShape.FilledCircle, 5, Colors.Red);
        var powerWorkPointMarker = aerodynamicPlot.Add.Marker(
                workPoint.flowRate,
                powerWorkPoint, MarkerShape.FilledCircle, 5, Colors.Red);

        powerWorkPointMarker.Axes.YAxis = powerPlot.Axes.YAxis;

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
        double posX = xMax * 0.75; // Такое же положение как у легенды
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

        var nominalTorquesPlot = torquePlot.Add.Scatter(rpmValues, nominalTorques, Colors.Black);
        nominalTorquesPlot.LegendText = "Момент при открытой заслонке";
        nominalTorquesPlot.LineWidth = 1;
        nominalTorquesPlot.MarkerSize = 0;

        var torqueWithGatesPlot = torquePlot.Add.Scatter(rpmValues, torqueWithGates, Colors.Grey);
        torqueWithGatesPlot.LegendText = "Момент при закрытой заслонке на входе";
        torqueWithGatesPlot.LinePattern = LinePattern.Dashed;
        torqueWithGatesPlot.LineWidth = 1;
        torqueWithGatesPlot.MarkerSize = 0;

        torquePlot.ShowLegend();
        torquePlot.Legend.Alignment = Alignment.UpperLeft;

        double xMin = 0;
        double xMax = rpmValues.Max() * 1.05; // Запас справа для информации
        double yMin = 0;
        double yMax = nominalTorques.Max() * 1.05;

        torquePlot.Axes.SetLimits(xMin, xMax, yMin, yMax);

        return torquePlot;
    }
    public static (double flowRate, double pressure) FindIntersectionPresurePoint(
           SpeedCalculationParameters parameters,
           List<AerodynamicsData> datas,
           int rpm)
    {
        var flowRateMax1 = parameters.FlowRateRequired * 2;

        var (flowRates, pressures) = CalculationDiagramHelper.GetStaticPressureMassive(
        pointsCount,
        parameters,
        datas,
        rpm);

        var (flowRates1, pressureResistances) = CalculationDiagramHelper.GetPressureResistanceMassive(
            pointsCount,
            parameters,
            flowRateMax1);

        // Интерполируем обе кривые для поиска пересечения
        var pressureCurve = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(flowRates, pressures);
        var resistanceCurve = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(flowRates1, pressureResistances);

        // Функция для поиска корня (разницы между кривыми)
        Func<double, double> diff = x => pressureCurve.Interpolate(x) - resistanceCurve.Interpolate(x);

        // Находим диапазон, где может быть пересечение
        double minX = Math.Max(flowRates.Min(), flowRates1.Min());
        double maxX = Math.Min(flowRates.Max(), flowRates1.Max());

        if (double.IsNaN(minX) || double.IsNaN(maxX) || maxX <= minX)
            return (double.NaN, double.NaN);

        int samples = 200;
        double step = (maxX - minX) / samples;
        double xPrev = minX;
        double fPrev = diff(xPrev);
        if (fPrev == 0)
            return (xPrev, pressureCurve.Interpolate(xPrev));

        for (int i = 1; i <= samples; i++)
        {
            double xCurr = (i == samples) ? maxX : minX + i * step;
            double fCurr = diff(xCurr);
            if (fCurr == 0)
                return (xCurr, pressureCurve.Interpolate(xCurr));
            if (Math.Sign(fPrev) != Math.Sign(fCurr))
            {
                double rootX = MathNet.Numerics.RootFinding.Brent.FindRoot(diff, xPrev, xCurr, 1e-5, 100);
                return (rootX, pressureCurve.Interpolate(rootX));
            }
            xPrev = xCurr;

            fPrev = fCurr;
        }
        return (double.NaN, double.NaN);
    }
    public static int GetRpmVFD
        (
        double pressureRequired,
        int rpm,
        double intersectionPressure
        )
    {
        return (int)Math.Ceiling(rpm * Math.Pow(pressureRequired / intersectionPressure, 0.5));
    }
}





