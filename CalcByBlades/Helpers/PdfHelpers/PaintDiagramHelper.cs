using ScottPlot;
using BladesCalc.Models;

namespace BladesCalc.Helpers.PdfHelpers;
public static class PaintDiagramHelper
{
    private const int PointsCount = 50;

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
            PointsCount,
            parameters,
            rpm,
            staticPressure1,
            staticPressure2,
            staticPressure3,
            minDeltaEfficiency,
            maxDeltaEfficiency,
            diameter);

        var (_, totalPressures) = CalculationDiagramHelper.GetTotalPressureMassive(
            PointsCount,
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
            PointsCount,
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
        staticPressurePlot.Color = Colors.DarkGray;
        staticPressurePlot.LineWidth = 2;
        staticPressurePlot.MarkerSize = 0;
        staticPressurePlot.LinePattern = LinePattern.Dotted;

        var totalPressurePlot = aerodynamicPlot.Add.Scatter(flowRates, totalPressures);
        totalPressurePlot.LegendText = "Полное давление";
        totalPressurePlot.Color = Colors.Black;
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
            PointsCount, flowRateForResistance, parameters);

        var resistancePlot = aerodynamicPlot.Add.Scatter(flowRates1, pressureResistances);
        resistancePlot.LegendText = "Характеристика сети";
        resistancePlot.Color = Colors.Blue;
        resistancePlot.LineWidth = 2;
        resistancePlot.MarkerSize = 0;

        // РАСЧЕТ ПРАВИЛЬНЫХ ПРЕДЕЛОВ ОСЕЙ
        double xMin = 0;
        double xMax = flowRates.Max() * 1.6; // Запас справа для информации
        double yMin = 0;
        double yMax = Math.Max(staticPressures.Max(), totalPressures.Max()) * 1.05;

        var totalPressure = totalPressureWorkPoint.pressure;

        var staticPressure = CalculationDiagramHelper.GetPolinomStaticPressure
                (totalPressureWorkPoint.flowRate,
                parameters,
                rpm,
                staticPressure1,
                staticPressure2,
                staticPressure3,
                diameter);
        var powerWorkPoint = CalculationDiagramHelper.GetPolinomPower(
                totalPressureWorkPoint.flowRate,
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
        var staticEfficiencyWorkPoint = CalculationDiagramHelper.GetPolinomStaticEficiency(
                totalPressureWorkPoint.flowRate,
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
                diameter) * 100;
        var totalEfficiencyWorkPoint = CalculationDiagramHelper.GetPolinomTotalEeficiency(
                totalPressureWorkPoint.flowRate,
                parameters,
                rpm,
                efficiency1,
                efficiency2,
                efficiency3,
                efficiency4,
                diameter) * 100;

        if (parameters.TypeOfPressure == 0)
        {
            totalPressure = CalculationDiagramHelper.GetPolinomTotalPressure
                (totalPressureWorkPoint.flowRate,
                parameters,
                rpm,
                staticPressure1,
                staticPressure2,
                staticPressure3,
                outletLength,
                outletWidth,
                diameter);
            staticPressure = totalPressureWorkPoint.pressure;
        }

        aerodynamicPlot.Add.Marker(
                totalPressureWorkPoint.flowRate,
                totalPressure, MarkerShape.FilledCircle, 5, Colors.Red);
        aerodynamicPlot.Add.Marker(
                totalPressureWorkPoint.flowRate,
                staticPressure, MarkerShape.FilledCircle, 5, Colors.Red);
        aerodynamicPlot.Add.Marker(
                parameters.FlowRateRequired,
                parameters.SystemResistance, MarkerShape.FilledCircle, 5, Colors.Red);
        var powerWorkPointMarker = aerodynamicPlot.Add.Marker(
                totalPressureWorkPoint.flowRate,
                powerWorkPoint, MarkerShape.FilledCircle, 5, Colors.Red);

        powerWorkPointMarker.Axes.YAxis = powerPlot.Axes.YAxis;
        // ЛЕГЕНДА СПРАВА НАПРОТИВ НАЗВАНИЯ ПРАВОЙ ОСИ (ПОНИЖЕ)
        aerodynamicPlot.ShowLegend();
        var legend = aerodynamicPlot.Legend;
        legend.Alignment = Alignment.UpperRight; // Справа вверху
        legend.FontSize = 14;

        // Добавляем отступ справа для информации
        aerodynamicPlot.Axes.Margins(right: 0.6);

        // ИНФОРМАЦИОННЫЙ БЛОК ПОД ЛЕГЕНДОЙ
        AddInfoBlockUnderLegend(
            aerodynamicPlot, 
            parameters, 
            staticPressure, 
            totalPressure, 
            xMax, 
            yMax,
            totalPressureWorkPoint,
            powerWorkPoint,
            staticEfficiencyWorkPoint,
            totalEfficiencyWorkPoint);

        rightAxis.Max = powers.Max() * 1.1;
        rightAxis.Min = 0;
        aerodynamicPlot.Axes.SetLimits(xMin, xMax, yMin, yMax);

        return aerodynamicPlot;
    }
   
    private static void AddInfoBlockUnderLegend(
        Plot plot, 
        BladesCalculationParameters parameters, 
        double staticPressure, 
        double totalPressure,  
        double xMax, 
        double yMax, 
        (double flowRate, double pressure) totalPressureWorkPoint,
        double powerWorkPoint,
        double staticEfficiencyWorkPoint,
        double totalEfficiencyWorkPoint)
    {
        // Создаем информационный блок
        string infoText =$"Заданная точка:\n" +
                         $"Расход: {parameters.FlowRateRequired:F1} м3/ч\n" +
                         $"Давление: {parameters.SystemResistance:F1} Па\n" +
                         $"Рабочая точка:\n" +
                         $"Статическое давление: {staticPressure:F1} Па\n" +
                         $"Статический КПД: {staticEfficiencyWorkPoint:F1} %\n" +
                         $"Полное давление: {totalPressure:F1} Па\n" +
                         $"Полный КПД: {totalEfficiencyWorkPoint:F1} %\n" +
                         $"Расход: {totalPressureWorkPoint.flowRate:F1} м³/ч\n" +
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

        if (parameters.NalichieVFD == true)
        {
            rpm = PaintDiagramsHelper.GetRpmVFD(
                parameters.SystemResistance,
                rpm,
                pressureWorkPoint.pressure);
        }

        var (rpmValues, nominalTorques) = CalculationDiagramHelper.GetNominalTorqueMassive(
            PointsCount,
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
            PointsCount,
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
        nominalTorque.Color = Colors.Black;
        nominalTorque.LineWidth = 1;
        nominalTorque.MarkerSize = 0;

        var torqueWithGatesPlot = torquePlot.Add.Scatter(rpmValues, torqueWithGates);
        torqueWithGatesPlot.LegendText = "Момент при закрытой заслонке на входе";
        torqueWithGatesPlot.Color = Colors.DarkGrey;
        torqueWithGatesPlot.LinePattern = LinePattern.Dashed;
        torqueWithGatesPlot.LineWidth = 1;
        torqueWithGatesPlot.MarkerSize = 0;

        torquePlot.ShowLegend();
        torquePlot.Legend.Alignment = Alignment.UpperLeft;

        double xMin = 0;
        double xMax = rpmValues.Max() * 1.05; // Запас справа для информации
        double yMin = 0;
        double yMax = nominalTorques.Max() * 1.05;

        torquePlot.Axes.SetLimits(xMin,xMax,yMin,yMax);

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
        PointsCount,
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
            PointsCount,
            flowRateMax1,
            parameters);

        if (parameters.TypeOfPressure == 0)
        {
            (flowRates, pressures) = CalculationDiagramHelper.GetStaticPressureMassive(
             PointsCount,
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

        int samples = 10;
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
