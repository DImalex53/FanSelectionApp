using ScottPlot;
using BladesCalc.Models;

namespace BladesCalc.Helpers.PdfHelpers;
public static class PaintDiagramHelper
{
    private const int pointsCount = 100;

    public static Plot GetDiagrameDraw(
     BladesCalculationParameters parameters,
     double staticPressure1,
     double staticPressure2,
     double staticPressure3,
     double minDeltaEfficiency,
     double maxDeltaEfficiency,
     double outletLength,
     double outletWidth,
     double efficiency1,
     double efficiency2,
     double efficiency3,
     double efficiency4,
     string newMarkOfFan,
     string newMarkOfFand,
     double diameter,
     int rpm)
    {
        var nameOfFan = parameters.SuctionType == 1 ? newMarkOfFand : newMarkOfFan;

        var (flowRates, staticPressures) = CalculationDiagramHelper.GetStaticPressureMassive(
            pointsCount,
            parameters,
            rpm,
            staticPressure1,
            staticPressure2,
            staticPressure3,
            minDeltaEfficiency,
            maxDeltaEfficiency,
            diameter);

        var (_, totalPressures) = CalculationDiagramHelper.GetTotalPressureMassive(
            pointsCount,
            parameters,
            rpm,
            staticPressure1,
            staticPressure2,
            staticPressure3,
            outletLength,
            outletWidth,
            minDeltaEfficiency,
            maxDeltaEfficiency,
            diameter);

        var (_, powers) = CalculationDiagramHelper.GetPowerMassive(
            pointsCount,
            parameters,
            rpm,
            staticPressure1,
            staticPressure2,
            staticPressure3,
            outletLength,
            outletWidth,
            efficiency1,
            efficiency2,
            efficiency3,
            efficiency4,
            minDeltaEfficiency,
            maxDeltaEfficiency,
            diameter);

        Plot aerodynamicPlot = new();

        // БАЗОВЫЕ НАСТРОЙКИ
        aerodynamicPlot.Title($"Вентилятор {nameOfFan}-{diameter * 10:F1}_{rpm} об/мин");
        aerodynamicPlot.XLabel("Расход воздуха, м³/ч");
        aerodynamicPlot.YLabel("Давление, Па");

        // Добавляем правую ось для мощности
        var rightAxis = aerodynamicPlot.Axes.Right;
        rightAxis.Label.Text = "Мощность, кВт";
        rightAxis.IsVisible = true;

        // ДОБАВЛЯЕМ ГРАФИКИ С ПОМОЩЬЮ Add.Scatter
        var staticPressurePlot = aerodynamicPlot.Add.Scatter(flowRates, staticPressures);
        staticPressurePlot.LegendText = "Статическое давление";
        staticPressurePlot.Color = Colors.Black;
        staticPressurePlot.LineWidth = 2;
        staticPressurePlot.MarkerSize = 0;

        var totalPressurePlot = aerodynamicPlot.Add.Scatter(flowRates, totalPressures);
        totalPressurePlot.LegendText = "Полное давление";
        totalPressurePlot.Color = Colors.DarkGray;
        totalPressurePlot.LineWidth = 2;
        totalPressurePlot.MarkerSize = 0;

        // График мощности на правой оси
        var powerPlot = aerodynamicPlot.Add.Scatter(flowRates, powers);
        powerPlot.LegendText = "Потребляемая мощность";
        powerPlot.Color = Colors.Orange;
        powerPlot.LineWidth = 2;
        powerPlot.MarkerSize = 0;
        powerPlot.Axes.YAxis = rightAxis;

        // Добавляем характеристику сети
        var totalPressureWorkPoint = FindIntersectionPresurePoint(
            parameters,
            staticPressure1,
            staticPressure2,
            staticPressure3,
            outletLength,
            outletWidth,
            minDeltaEfficiency,
            maxDeltaEfficiency,
            diameter,
            rpm);

        double flowRateForResistance = double.IsNaN(totalPressureWorkPoint.flowRate)
            ? parameters.FlowRateRequired
            : totalPressureWorkPoint.flowRate;

        var (flowRates1, pressureResistances) = CalculationDiagramHelper.GetPressureResistanceMassive(
            pointsCount, flowRateForResistance, parameters);

        var resistancePlot = aerodynamicPlot.Add.Scatter(flowRates1, pressureResistances);
        resistancePlot.LegendText = "Характеристика сети";
        resistancePlot.Color = Colors.Gray;
        resistancePlot.LineWidth = 2;
        resistancePlot.MarkerSize = 0;

        // РАСЧЕТ ПРАВИЛЬНЫХ ПРЕДЕЛОВ ОСЕЙ
        double xMin = 0;
        double xMax = flowRates.Max() * 1.25; // Запас справа для информации
        double yMin = 0;
        double yMax = Math.Max(staticPressures.Max(), totalPressures.Max()) * 1.05;

        aerodynamicPlot.Axes.SetLimitsX(xMin, xMax);
        aerodynamicPlot.Axes.SetLimitsY(yMin, yMax);

        // ЛЕГЕНДА СПРАВА НАПРОТИВ НАЗВАНИЯ ПРАВОЙ ОСИ (ПОНИЖЕ)
        aerodynamicPlot.ShowLegend();
        var legend = aerodynamicPlot.Legend;
        legend.Alignment = Alignment.MiddleRight; // По центру справа, напротив мощности

        // Добавляем отступ справа для информации
        aerodynamicPlot.Axes.Margins(right: 0.3);

        // ИНФОРМАЦИОННЫЙ БЛОК ПРЯМО ПОД ЛЕГЕНДОЙ
        AddInfoBlockUnderLegend(aerodynamicPlot, parameters, staticPressures.Last(), totalPressures.Last(), flowRates.Last(), xMax, yMax);

        return aerodynamicPlot;
    }

    private static void AddInfoBlockUnderLegend(Plot plot, BladesCalculationParameters parameters, double staticPressure, double totalPressure, double flowRate, double xMax, double yMax)
    {
        // Создаем информационный блок
        string infoText = $"Статическое давление: {staticPressure:F1} Па\n" +
                         $"Полное давление: {totalPressure:F1} Па\n" +
                         $"Расход: {flowRate:F1} м³/ч\n" +
                         $"Плотность на входе: {parameters.Density:F2} кг/м³";

        // Размещаем прямо под легендой с небольшим зазором
        // Такое же положение по X как у легенды (справа с отступом)
        double posX = xMax * 0.78; // Такое же положение как у легенды
        double posY = yMax * 0.25; // Прямо под легендой с небольшим зазором

        var text = plot.Add.Text(infoText, posX, posY);
        text.Color = Colors.Black; // Черный цвет текста
        text.FontSize = 11;
        text.Alignment = Alignment.UpperLeft;
        text.BackgroundColor = Colors.White; // Белый фон
        text.BorderColor = Colors.Black; // Черная рамка
        text.Padding = 5;
    }

    public static byte[] GetDiagramAsImageBytes(
        BladesCalculationParameters parameters,
        double staticPressure1,
        double staticPressure2,
        double staticPressure3,
        double minDeltaEfficiency,
        double maxDeltaEfficiency,
        double outletLength,
        double outletWidth,
        double efficiency1,
        double efficiency2,
        double efficiency3,
        double efficiency4,
        string newMarkOfFan,
        string newMarkOfFand,
        double diameter,
        int rpm,
        int width,
        int height,
        string imageFormat)
    {
        var plot = GetDiagrameDraw(
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

        if (plot == null)
            return Array.Empty<byte>();

        var format = imageFormat.ToLower() switch
        {
            "png" => ImageFormat.Png,
            "jpeg" or "jpg" => ImageFormat.Jpeg,
            "bmp" => ImageFormat.Bmp,
            _ => throw new ArgumentException("Unsupported format")
        };

        return plot.GetImageBytes(width, height, format);
    }

    public static Plot GenerateTorquePlot(
        BladesCalculationParameters parameters,
        double staticPressure1,
        double staticPressure2,
        double staticPressure3,
        double minDeltaEfficiency,
        double maxDeltaEfficiency,
        double outletLength,
        double outletWidth,
        double efficiency1,
        double efficiency2,
        double efficiency3,
        double efficiency4,
        string newMarkOfFan,
        string newMarkOfFand,
        double diameter,
        int rpm)
    {
        int pointsCount = 100;
        var pressureWorkPoint = FindIntersectionPresurePoint(
            parameters,
            staticPressure1,
            staticPressure2,
            staticPressure3,
            outletLength,
            outletWidth,
            minDeltaEfficiency,
            maxDeltaEfficiency,
            diameter,
            rpm);

        var (rpmValues, nominalTorques) = CalculationDiagramHelper.GetNominalTorqueMassive(
            pointsCount,
            pressureWorkPoint.flowRate,
            parameters,
            rpm,
            staticPressure1,
            staticPressure2,
            staticPressure3,
            outletLength,
            outletWidth,
            efficiency1,
            efficiency2,
            efficiency3,
            efficiency4,
            diameter);

        var (_, torqueWithGates) = CalculationDiagramHelper.GetTorqueWithGateMassive(
            pointsCount,
            pressureWorkPoint.flowRate,
            parameters,
            rpm,
            staticPressure1,
            staticPressure2,
            staticPressure3,
            outletLength,
            outletWidth,
            efficiency1,
            efficiency2,
            efficiency3,
            efficiency4,
            diameter);

        var torquePlot = new Plot();
        torquePlot.Title($"Нагрузочная характеристика электродвигателя");
        torquePlot.XLabel("Обороты рабочего колеса, об/мин");
        torquePlot.YLabel("Момент силы, кН*м");

        var nominalTorque = torquePlot.Add.Scatter(rpmValues, nominalTorques);
        nominalTorque.LegendText = "Момент при открытой заслонке";
        nominalTorque.Color = Colors.Grey;
        nominalTorque.LineWidth = 1;
        nominalTorque.MarkerSize = 0;

        var torqueWithGatesPlot = torquePlot.Add.Scatter(rpmValues, torqueWithGates);
        torqueWithGatesPlot.LegendText = "Момент при закрытой заслонке на входе";
        torqueWithGatesPlot.Color = Colors.Black;
        torqueWithGatesPlot.LineWidth = 1;
        torqueWithGatesPlot.MarkerSize = 0;

        torquePlot.ShowLegend();
        torquePlot.Legend.Alignment = Alignment.LowerRight;

        return torquePlot;
    }

    public static (double flowRate, double pressure) FindIntersectionPresurePoint(
            BladesCalculationParameters parameters,
            double staticPressure1,
            double staticPressure2,
            double staticPressure3,
            double outletLength,
            double outletWidth,
            double minDeltaEfficiency,
            double maxDeltaEfficiency,
            double diameter,
            int rpm)
    {
        var flowRateMax1 = parameters.FlowRateRequired * 2;

        var (flowRates, pressures) = CalculationDiagramHelper.GetTotalPressureMassive(
        pointsCount,
        parameters,
        rpm,
        staticPressure1,
        staticPressure2,
        staticPressure3,
        outletLength,
        outletWidth,
        minDeltaEfficiency,
        maxDeltaEfficiency,
        diameter);

        var (flowRates1, pressureResistances) = CalculationDiagramHelper.GetPressureResistanceMassive(
            pointsCount,
            flowRateMax1,
            parameters);

        if (parameters.TypeOfPressure == 0)
        {
            (flowRates, pressures) = CalculationDiagramHelper.GetStaticPressureMassive(
             pointsCount,
             parameters,
             rpm,
             staticPressure1,
             staticPressure2,
             staticPressure3,
             minDeltaEfficiency,
             maxDeltaEfficiency,
             diameter);
        }

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
}
