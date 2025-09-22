using ScottPlot;
using BladesCalc.Models;

namespace BladesCalc.Helpers.PdfHelpers;
public static class PaintDiagramHelper
{
    private const int pointsCount = 100;
    public static Plot? GetDiagrameDraw(
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
     int rpm
     )
    {
        var nameOfFan = newMarkOfFan;

        if (parameters.SuctionType == 1)
        {
            nameOfFan = newMarkOfFand;
        }

        var (flowRates, staticPressures) = CalculationDiagramHelper.GetStaticPressureMassive(
            pointsCount,
            parameters,
            rpm,
            staticPressure1,
            staticPressure2,
            staticPressure3,
            minDeltaEfficiency,
            maxDeltaEfficiency,
            diameter
            );
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
            diameter
            );
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
            diameter
            );

        var aerodynamicPlot = new Plot();
        aerodynamicPlot.Title($"Вентилятор {nameOfFan}-{diameter * 10:F1}_{rpm} об/мин");
        aerodynamicPlot.XLabel("Расход воздуха, м³/ч");
        aerodynamicPlot.Axes.Left.Label.Text = "Давление, Па";
        aerodynamicPlot.Axes.Right.Label.Text = "Мощность, кВт";

        // Добавляем основные графики
        var staticPressurePlot = aerodynamicPlot.Add.Scatter(flowRates, staticPressures, Colors.Grey);
        staticPressurePlot.LegendText = "Статическое давление";

        var totalPressurePlot = aerodynamicPlot.Add.Scatter(flowRates, totalPressures, Colors.Black);
        totalPressurePlot.LegendText = "Полное давление";

        var powerPlot = aerodynamicPlot.Add.Scatter(flowRates, powers, Colors.Orange);
        powerPlot.LegendText = "Потребляемая мощность";
        powerPlot.Axes.YAxis = aerodynamicPlot.Axes.Right;

        // Добавляем характеристику сети (если нужно)
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

        var resistancePlot = aerodynamicPlot.Add.Scatter(flowRates1, pressureResistances, Colors.Blue);
        resistancePlot.LegendText = "Характеристика сети";

        // Создаем информационный текст для правой стороны
        string infoText = $"Статическое давление: {staticPressures.Last():F1} Па\n" +
                         $"Полное давление: {totalPressures.Last():F1} Па\n" +
                         $"Расход: {flowRates.Last():F1} м³/ч\n" +
                         $"Плотность на входе: {parameters.Density:F2} кг/м³";

        // Добавляем информационный текст справа (координаты относительно размеров графика)
        var infoTextPlot = aerodynamicPlot.Add.Text(infoText, 0.75, 0.8);
        infoTextPlot.Color = Colors.DarkBlue;
        infoTextPlot.FontSize = 12;
        infoTextPlot.Bold = true;
        infoTextPlot.Alignment = Alignment.UpperLeft;
        infoTextPlot.BackgroundColor = Colors.LightGray;
        infoTextPlot.BorderColor = Colors.Gray;
        infoTextPlot.BorderWidth = 1;
        infoTextPlot.Padding = 5;

        // Вычисляем правильные пределы осей
        var allPlottables = aerodynamicPlot.GetPlottables();

        if (allPlottables.Any())
        {
            double xMin = 0;
            double xMax = allPlottables.Max(p => p.GetAxisLimits().Right);
            double yMin = 0;
            double yMax = allPlottables.Max(p => p.GetAxisLimits().Top);

            // Добавляем зазор 5%
            aerodynamicPlot.Axes.SetLimits(
                left: xMin,
                right: xMax * 1.05,
                bottom: yMin,
                top: yMax * 1.05
            );
        }
        else
        {
            // Запасные значения
            aerodynamicPlot.Axes.SetLimits(0, 1000, 0, 1000);
        }

        // Легенда
        aerodynamicPlot.ShowLegend();
        aerodynamicPlot.Legend.Alignment = Alignment.LowerRight;

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
        string imageFormat )
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
    public static Plot? GenerateTorquePlot(
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
        torquePlot.Axes.Left.Label.Text = "Момент силы, кН*м";

        var nominalTorque = torquePlot.Add.Scatter(rpmValues, nominalTorques, Colors.Grey);
        nominalTorque.LegendText = "Момент при открытой заслонке";

        var torqueWithGatesPlot = torquePlot.Add.Scatter(rpmValues, torqueWithGates, Colors.Black);
        torqueWithGatesPlot.LegendText = "Момент при закрытой заслонке на входе";

        torquePlot.ShowLegend();
        torquePlot.Legend.Alignment = Alignment.LowerRight;

        return torquePlot;
    }

    private static void AddWorkPointMarker(Plot aerodynamicPlot, double x, double y, string annotationText, string legendText,
                        int offSetX, int offSetY, Alignment alignment)
    {
        var marker = aerodynamicPlot.Add.Marker(x, y, MarkerShape.FilledCircle, 15, Colors.Red);
        marker.LegendText = legendText;

        var annotation = aerodynamicPlot.Add.Annotation(annotationText);
        annotation.Alignment = alignment;
        annotation.OffsetX = offSetX;
        annotation.OffsetY = offSetY;
        annotation.LabelFontColor = Colors.Red;
        annotation.LabelBorderColor = Colors.LightGray;
        annotation.LabelBorderWidth = 1;
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
        diameter
        );

            var (flowRates1, pressureResistances) = CalculationDiagramHelper.GetPressureResistanceMassive(
                pointsCount,
                flowRateMax1,
                parameters
                );
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
            diameter
            );
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

