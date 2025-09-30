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
        aerodynamicPlot.Title($"Вентилятор {nameOfFan}-{diameter * 10:F1}_{rpm} об/мин");
        aerodynamicPlot.XLabel("Расход воздуха, м³/ч");
        aerodynamicPlot.YLabel("Давление, Па");

        // Добавляем правую ось для мощности
        var rightAxis = aerodynamicPlot.Axes.Right;
        rightAxis.Label.Text = "Мощность, кВт";
        rightAxis.IsVisible = true;

        // Добавляем основные графики
        var staticPressurePlot = aerodynamicPlot.Add.Scatter(flowRates, staticPressures);
        staticPressurePlot.LegendText = "Статическое давление";
        staticPressurePlot.Color = Colors.Black;
        staticPressurePlot.LineStyle = new LineStyle() { Pattern = LinePattern.Solid, Width = 1 };

        var totalPressurePlot = aerodynamicPlot.Add.Scatter(flowRates, totalPressures);
        totalPressurePlot.LegendText = "Полное давление";
        totalPressurePlot.Color = Colors.Black;
        totalPressurePlot.LineStyle = new LineStyle() { Pattern = LinePattern.Solid, Width = 1 };

        var powerPlot = aerodynamicPlot.Add.Scatter(flowRates, powers);
        powerPlot.LegendText = "Потребляемая мощность";
        powerPlot.Color = Colors.Orange;
        powerPlot.LinePattern = LinePattern.Solid;
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
        resistancePlot.LineStyle = new LineStyle() { Pattern = LinePattern.Solid, Width = 1 };

        // Добавляем рабочую точку в легенду (скрытый маркер)
        var workPointPlot = aerodynamicPlot.Add.Marker(0, 0);
        workPointPlot.LegendText = "Рабочая точка";
        workPointPlot.Color = Colors.Red;
        workPointPlot.MarkerSize = 8;
        workPointPlot.MarkerShape = MarkerShape.FilledCircle;
        workPointPlot.IsVisible = false;

        // Создаем информационный текст
        string infoText = $"Статическое давление: {staticPressures.Last():F1} Па\n" +
                         $"Полное давление: {totalPressures.Last():F1} Па\n" +
                         $"Расход: {flowRates.Last():F1} м³/ч\n" +
                         $"Плотность на входе: {parameters.Density:F2} кг/м³";

        // Добавляем информационный текст с фоном
        double textX = flowRates.Max() * 0.75;
        double textY = totalPressures.Max() * 0.8;
        var Coordinate = new Coordinates(textX, textY);
        var text = aerodynamicPlot.Add.Text(infoText, Coordinate);
        text.LabelFontColor = Colors.DarkBlue;
        text.LabelFontSize = 12;
        text.LabelBold = true;
        text.Alignment = Alignment.UpperLeft;
        text.LabelBackgroundColor = Colors.LightGray;
        text.LabelBorderColor = Colors.Gray;
        text.LabelBorderWidth = 1;
        text.LabelPadding = 5;

        // Устанавливаем пределы осей
        double xMin = 0;
        double xMax = flowRates.Max() * 1.05;
        double yMin = 0;
        double yMax = Math.Max(staticPressures.Max(), totalPressures.Max()) * 1.05;

        aerodynamicPlot.Axes.SetLimitsX(xMin, xMax);
        aerodynamicPlot.Axes.SetLimitsY(yMin, yMax);

        // Настраиваем легенду
        aerodynamicPlot.ShowLegend();
        var legend = aerodynamicPlot.Legend;
        legend.Alignment = Alignment.UpperRight;

        // Добавляем реальную рабочую точку на график
        if (!double.IsNaN(totalPressureWorkPoint.flowRate) && !double.IsNaN(totalPressureWorkPoint.pressure))
        {
            AddWorkPointMarker(
                aerodynamicPlot,
                totalPressureWorkPoint.flowRate,
                totalPressureWorkPoint.pressure,
                "Рабочая точка");
        }

        return aerodynamicPlot;
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
            "png" => ScottPlot.ImageFormat.Png,
            "jpeg" or "jpg" => ScottPlot.ImageFormat.Jpeg,
            "bmp" => ScottPlot.ImageFormat.Bmp,
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

        var torqueWithGatesPlot = torquePlot.Add.Scatter(rpmValues, torqueWithGates);
        torqueWithGatesPlot.LegendText = "Момент при закрытой заслонке на входе";
        torqueWithGatesPlot.Color = Colors.Black;
        torqueWithGatesPlot.LineWidth = 1;

        torquePlot.ShowLegend();
        torquePlot.Legend.Alignment = Alignment.LowerRight;

        return torquePlot;
    }

    private static void AddWorkPointMarker(Plot aerodynamicPlot, double x, double y, string label)
    {
        var marker = aerodynamicPlot.Add.Marker(x, y);
        marker.LegendText = label;
        marker.Color = Colors.Red;
        marker.MarkerSize = 10;
        marker.MarkerShape = MarkerShape.FilledCircle;

        // Добавляем текст рядом с маркером
        var Coordinate = new Coordinates(x + x * 0.05, y + y * 0.05);
        var text = aerodynamicPlot.Add.Text(label, Coordinate);
        text.LabelFontColor = Colors.Red;
        text.LabelFontSize = 10;
        text.Alignment = Alignment.LowerLeft;
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
