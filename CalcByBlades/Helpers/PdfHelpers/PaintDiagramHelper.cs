using BladesCalc.Helpers.AerodynamicHelpers;
using ScottPlot;
using System.IO;
using static iText.IO.Image.Jpeg2000ImageData;
using System.Runtime.Intrinsics.Arm;
using BladesCalc.Models;
using BladesCalc.Helpers.PdfHelpers;
using System.Data;

namespace BladesCalc.Helpers.PdfHelpers;
public static class PaintDiagramHelper
{
    private const int pointsCount = 100;
    public static Plot? GetDiagrameDraw(
        CalculationParameters parameters,
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
        aerodynamicPlot.Title($"Вентилятор {nameOfFan}-{diameter * 10:F1}_{rpm} об/мин_плотность " +
                        $"{parameters.Density} кг/м3");
        aerodynamicPlot.XLabel("Расход воздуха, м³/ч");
        aerodynamicPlot.Axes.Left.Label.Text = "Давление, Па";
        aerodynamicPlot.Axes.Right.Label.Text = "Мощность, кВт";

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
        var (flowRates1, pressureResistances) = CalculationDiagramHelper.GetPressureResistanceMassive(pointsCount, totalPressureWorkPoint.flowRate, parameters);           

        if (parameters.TypeOfPressure == 1)
        {
            var staticPressureWorkPoint = CalculationDiagramHelper.GetPolinomStaticPressure (
                totalPressureWorkPoint.flowRate,
                parameters,
                rpm,
                staticPressure1,
                staticPressure2,
                staticPressure3,
                diameter
                );
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
                diameter
                );
            var totalEficiencyWorkPoint = CalculationDiagramHelper.GetPolinomTotalEeficiency(
                totalPressureWorkPoint.flowRate,
                parameters,
                rpm,
                efficiency1,
                efficiency2,
                efficiency3,
                efficiency4,
                diameter
                );
            var staticEficiencyWorkPoint = CalculationDiagramHelper.GetPolinomStaticEficiency(
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
                diameter
                );
            if (!double.IsNaN(totalPressureWorkPoint.flowRate))
            {
                AddWorkPointMarker(aerodynamicPlot, totalPressureWorkPoint.flowRate, totalPressureWorkPoint.pressure,
                $"Q = {totalPressureWorkPoint.flowRate:F1} м³/ч\nPv = {totalPressureWorkPoint.pressure:F1} Па\n" +
                $"КПД = {totalEficiencyWorkPoint:F2}\nМощность = {powerWorkPoint:F1}",
                "Полное давление", 0, 0, Alignment.LowerLeft);

                AddWorkPointMarker(aerodynamicPlot, totalPressureWorkPoint.flowRate, staticPressureWorkPoint,
                    $"Q = {totalPressureWorkPoint.flowRate:F1} м³/ч\nPs = {staticPressureWorkPoint:F1} Па\n" +
                    $"КПД = {staticEficiencyWorkPoint:F2}\nМощность = {powerWorkPoint:F1}",
                    "Статическое давление", 0, 0, Alignment.LowerLeft);
            }
        }
        
        if (parameters.TypeOfPressure == 0)
        { 
            var staticPressureWorkPoint = FindIntersectionPresurePoint(
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
          (flowRates1, pressureResistances) = CalculationDiagramHelper.GetPressureResistanceMassive(
              pointsCount, 
              staticPressureWorkPoint.flowRate, 
              parameters);
            var powerWorkPoint = CalculationDiagramHelper.GetPolinomPower(
                staticPressureWorkPoint.flowRate,
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
                diameter
                );
            var totalEficiencyWorkPoint = CalculationDiagramHelper.GetPolinomTotalEeficiency(
                staticPressureWorkPoint.flowRate,
                parameters,
                rpm,
                efficiency1,
                efficiency2,
                efficiency3,
                efficiency4,
                diameter
                );
            var staticEficiencyWorkPoint = CalculationDiagramHelper.GetPolinomStaticEficiency(
                staticPressureWorkPoint.flowRate,
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
                diameter
                );
            var totalPressureWorkPoint1 = CalculationDiagramHelper.GetPolinomTotalPressure(
                staticPressureWorkPoint.flowRate,
                parameters,
                rpm,
                staticPressure1,
                staticPressure2,
                staticPressure3,
                outletLength,
                outletWidth,                
                diameter
                );

            if (!double.IsNaN(staticPressureWorkPoint.flowRate))
            {
                AddWorkPointMarker(aerodynamicPlot, staticPressureWorkPoint.flowRate, totalPressureWorkPoint1,
                    $"Q = {staticPressureWorkPoint.flowRate:F1} м³/ч\nPv = {totalPressureWorkPoint1:F1} Па\n" +
                    $"КПД = {totalEficiencyWorkPoint:F2}\nМощность = {powerWorkPoint:F1}",
                    "Полное давление", 0, 0, Alignment.LowerLeft);

                AddWorkPointMarker(aerodynamicPlot, staticPressureWorkPoint.flowRate, staticPressureWorkPoint.flowRate,
                    $"Q = {staticPressureWorkPoint.flowRate:F1} м³/ч\nPs = {staticPressureWorkPoint.pressure:F1} Па\n" +
                    $"КПД = {staticEficiencyWorkPoint:F2}\nМощность = {powerWorkPoint:F1}",
                    "Статическое давление", 0, 0, Alignment.LowerLeft);
            }
        }

        double xMax = aerodynamicPlot.GetPlottables().Max(p => p.GetAxisLimits().Right);
        double yMax = aerodynamicPlot.GetPlottables().Max(p => p.GetAxisLimits().Top);
        aerodynamicPlot.Axes.SetLimits(
            left: 0,
            right: xMax * 1.05,
            bottom: 0,
            top: yMax * 1.05
        );
        aerodynamicPlot.Axes.Bottom.Min = 0;

        var staticPressurePlot = aerodynamicPlot.Add.Scatter(flowRates, staticPressures, Colors.Grey);
        staticPressurePlot.LegendText = "Статическое давление";

        var totalPressurePlot = aerodynamicPlot.Add.Scatter(flowRates, totalPressures, Colors.Black);
        totalPressurePlot.LegendText = "Полное давление";

        var resistancePlot = aerodynamicPlot.Add.Scatter(flowRates, pressureResistances, Colors.Blue);
        resistancePlot.LegendText = "Характеристика сети";

        var powerPlot = aerodynamicPlot.Add.Scatter(flowRates, powers, Colors.Orange);
        powerPlot.LegendText = "Потребляемая мощность";
        powerPlot.Axes.YAxis = aerodynamicPlot.Axes.Right;

        aerodynamicPlot.ShowLegend();
        aerodynamicPlot.Legend.Alignment = Alignment.LowerRight;

        return aerodynamicPlot;
    }
    public static byte[] GetDiagramAsImageBytes(
    CalculationParameters parameters,
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
    string imageFormat
        )
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
        CalculationParameters parameters,
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
            CalculationParameters parameters,
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

        // Используем метод Брента для поиска пересечения
        try
        {
            double intersectionX = MathNet.Numerics.RootFinding.Brent.FindRoot(diff, minX, maxX, 1e-5, 100);
            double intersectionY = pressureCurve.Interpolate(intersectionX);
            return (intersectionX, intersectionY);
        }
        catch
        {
            // Если пересечение не найдено
            return (double.NaN, double.NaN);
        }
    }
}

