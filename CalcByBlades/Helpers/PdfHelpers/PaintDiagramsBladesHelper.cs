using BladesCalc.Models;

namespace BladesCalc.Helpers.PdfHelpers;
public static class PaintDiagramsHelper
{
    private const int MaxRPM = 3000;
    private const double MinDiameter = 0.4;
    private const double MaxDiameter = 3;
    private const int MaxHalfPoluces = 6;
    private const int MinRPM = 100;

    public static List<DatasRightVents> GenerateTableOfRightVents
        (
        List<AerodynamicsDataBlades> aerodynamicsByTypeBlades, 
        BladesCalculationParameters parameters, 
        ParametersDrawImage parametersDrawImage
        )
    {
        var resultRightVents = new List<DatasRightVents>();
        int numberOfVent = 0;

        foreach (var aerodynamicRow in aerodynamicsByTypeBlades)
        {
            var rightSchemes = parameters.NalichieVFD == true ? 
                GetRightVentsVFD(aerodynamicRow, parameters, parametersDrawImage, numberOfVent) : 
                GetRightVents(aerodynamicRow, parameters, parametersDrawImage, numberOfVent);

            if (rightSchemes.Any())
            {
                resultRightVents.AddRange(rightSchemes);
            }
            numberOfVent += rightSchemes.Count();
        }

        return resultRightVents;
    }

    private static List<DatasRightVents> GetRightVents
        (
        AerodynamicsDataBlades aerodynamicRow, 
        BladesCalculationParameters parameters,
        ParametersDrawImage parametersDrawImage,
        int numberOfVent
        )
    {
        var result = new List<DatasRightVents>();
        var staticPressure1 = aerodynamicRow.StaticPressure1;
        var staticPressure2 = aerodynamicRow.StaticPressure2;
        var staticPressure3 = aerodynamicRow.StaticPressure3;
        var minDeltaEfficiency = aerodynamicRow.MinDeltaEfficiency;
        var maxDeltaEfficiency = aerodynamicRow.MaxDeltaEfficiency;
        var outletLength = aerodynamicRow.OutletLength;
        var outletWidth = aerodynamicRow.OutletWidth;
        var efficiency1 = aerodynamicRow.Efficiency1;
        var efficiency2 = aerodynamicRow.Efficiency2;
        var efficiency3 = aerodynamicRow.Efficiency3;
        var efficiency4 = aerodynamicRow.Efficiency4;
        var newMarkOfFan = aerodynamicRow.NewMarkOfFan;
        var newMarkOfFand = aerodynamicRow.NewMarkOfFand;
        var width = parametersDrawImage.Width;
        var height = parametersDrawImage.Height;
        var imageFormat = parametersDrawImage.ImageFormat;

        var nameOfFan = parameters.SuctionType == 1 ? aerodynamicRow.NewMarkOfFand : aerodynamicRow.NewMarkOfFan;

        double diameter = MinDiameter;
        int minHalfPoluces = 1;
        int rpm;
        for (int k = 0; diameter < MaxDiameter; k++)
        {
            for (int halfPoluces = MaxHalfPoluces; halfPoluces > 0; halfPoluces--)
            {
                if (halfPoluces < minHalfPoluces)
                {
                    continue;
                }

                rpm = MaxRPM / halfPoluces;

                var pressure = parameters.TypeOfPressure == 1
                    ? CalculationDiagramHelper.GetPolinomTotalPressure(
                        parameters.FlowRateRequired,
                        parameters,
                        rpm,
                        staticPressure1,
                        staticPressure2,
                        staticPressure3,
                        outletLength,
                        outletWidth,
                        diameter)
                    : CalculationDiagramHelper.GetPolinomStaticPressure(
                        parameters.FlowRateRequired,
                        parameters,
                        rpm,
                        staticPressure1,
                        staticPressure2,
                        staticPressure3,
                        diameter);

                var totalPressureWorkPoint = PaintDiagramHelper.FindIntersectionPresurePoint(
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
                var totalEfficiency = CalculationDiagramHelper.GetPolinomTotalEeficiency(
                    totalPressureWorkPoint.flowRate, 
                    parameters, 
                    rpm, 
                    efficiency1, 
                    efficiency2, 
                    efficiency3, 
                    efficiency4, 
                    diameter);

                if (parameters.SystemResistance > pressure)
                {
                    continue;
                }

                var flowRateMax = CalculationDiagramHelper.GetFlowRateMaxMin(
                    aerodynamicRow.MaxDeltaEfficiency,
                    diameter,
                    rpm,
                    parameters
                    );

                if (parameters.FlowRateRequired > flowRateMax)
                {
                    continue;
                }

                var flowRateMin = CalculationDiagramHelper.GetFlowRateMaxMin(
                    aerodynamicRow.MinDeltaEfficiency,
                    diameter,
                    rpm,
                    parameters
                    );

                if (parameters.FlowRateRequired < flowRateMin)
                {
                    continue;
                }

                if (double.IsNaN(PaintDiagramHelper.FindIntersectionPresurePoint(
                    parameters,
                    staticPressure1,
                    staticPressure2,
                    staticPressure3,
                    outletLength,
                    outletWidth,
                    minDeltaEfficiency,
                    maxDeltaEfficiency,
                    diameter,
                    rpm).flowRate))
                {
                    continue;
                }

                var aerodynamicPlotBytes = PaintDiagramHelper.GetDiagramAsImageBytes(
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
                    rpm,
                    width,
                    height,
                    imageFormat);

                numberOfVent++;
                result.Add(new DatasRightVents
                {
                    Diameter = diameter,
                    Rpm = rpm,
                    Scheme = aerodynamicRow.Scheme,
                    NumberOfVent = numberOfVent,
                    DiagramAsImageBytes = aerodynamicPlotBytes,
                    NewMarkOfFan = nameOfFan,
                    TotalEfficiency = totalEfficiency,
                });

                minHalfPoluces++;
            }

            diameter += 0.05;
        }

        return result;
    }

public static int GetRpmVFD
        (
        double pressureRequired,
        int rpm,
        double intersectionPressure
        )
    {
        return (int) Math.Ceiling(rpm * Math.Pow(pressureRequired / intersectionPressure, 0.5));
    }
    private static List<DatasRightVents> GetRightVentsVFD
        (
        AerodynamicsDataBlades aerodynamicRow,
        BladesCalculationParameters parameters,
        ParametersDrawImage parametersDrawImage,
        int numberOfVent
        )
    {
        var result = new List<DatasRightVents>();
        var staticPressure1 = aerodynamicRow.StaticPressure1;
        var staticPressure2 = aerodynamicRow.StaticPressure2;
        var staticPressure3 = aerodynamicRow.StaticPressure3;
        var minDeltaEfficiency = aerodynamicRow.MinDeltaEfficiency;
        var maxDeltaEfficiency = aerodynamicRow.MaxDeltaEfficiency;
        var outletLength = aerodynamicRow.OutletLength;
        var outletWidth = aerodynamicRow.OutletWidth;
        var efficiency1 = aerodynamicRow.Efficiency1;
        var efficiency2 = aerodynamicRow.Efficiency2;
        var efficiency3 = aerodynamicRow.Efficiency3;
        var efficiency4 = aerodynamicRow.Efficiency4;
        var newMarkOfFan = aerodynamicRow.NewMarkOfFan;
        var newMarkOfFand = aerodynamicRow.NewMarkOfFand;
        var width = parametersDrawImage.Width;
        var height = parametersDrawImage.Height;
        var imageFormat = parametersDrawImage.ImageFormat;

        var nameOfFan = parameters.SuctionType == 1 ? aerodynamicRow.NewMarkOfFand : aerodynamicRow.NewMarkOfFan;

        double diameter = MinDiameter;

        for (int k = 0; diameter < MaxDiameter; k++)
        {
            for (int rpm = MinRPM; rpm <= MaxRPM; rpm+=100)
            {

                var pressure = parameters.TypeOfPressure == 1
                    ? CalculationDiagramHelper.GetPolinomTotalPressure(
                        parameters.FlowRateRequired,
                        parameters,
                        rpm,
                        staticPressure1,
                        staticPressure2,
                        staticPressure3,
                        outletLength,
                        outletWidth,
                        diameter)
                    : CalculationDiagramHelper.GetPolinomStaticPressure(
                        parameters.FlowRateRequired,
                        parameters,
                        rpm,
                        staticPressure1,
                        staticPressure2,
                        staticPressure3,
                        diameter);

                var pressureWorkPoint = PaintDiagramHelper.FindIntersectionPresurePoint(
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
                var totalEfficiency = CalculationDiagramHelper.GetPolinomTotalEeficiency(
                    pressureWorkPoint.flowRate,
                    parameters,
                    rpm,
                    efficiency1,
                    efficiency2,
                    efficiency3,
                    efficiency4,
                    diameter);

                var flowRateMax = CalculationDiagramHelper.GetFlowRateMaxMin(
                    aerodynamicRow.MaxDeltaEfficiency,
                    diameter,
                    rpm,
                    parameters
                    );

                if (parameters.FlowRateRequired > flowRateMax)
                {
                    continue;
                }

                var flowRateMin = CalculationDiagramHelper.GetFlowRateMaxMin(
                    aerodynamicRow.MinDeltaEfficiency,
                    diameter,
                    rpm,
                    parameters
                    );

                if (parameters.FlowRateRequired < flowRateMin)
                {
                    continue;
                }

                if (double.IsNaN(PaintDiagramHelper.FindIntersectionPresurePoint(
                    parameters,
                    staticPressure1,
                    staticPressure2,
                    staticPressure3,
                    outletLength,
                    outletWidth,
                    minDeltaEfficiency,
                    maxDeltaEfficiency,
                    diameter,
                    rpm).flowRate))
                {
                    continue;
                }

                    rpm = PaintDiagramsHelper.GetRpmVFD(
                        parameters.SystemResistance,
                        rpm,
                        PaintDiagramHelper.FindIntersectionPresurePoint(
                            parameters,
                            staticPressure1,
                            staticPressure2,
                            staticPressure3,
                            outletLength,
                            outletWidth,
                            minDeltaEfficiency,
                            maxDeltaEfficiency,
                            diameter,
                            rpm).pressure);

                if (rpm > MaxRPM)
                {
                    continue;
                }

                var aerodynamicPlotBytes = PaintDiagramHelper.GetDiagramAsImageBytes(
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
                    rpm,
                    width,
                    height,
                    imageFormat);

                numberOfVent++;
                result.Add(new DatasRightVents
                {
                    Diameter = diameter,
                    Rpm = rpm,
                    Scheme = aerodynamicRow.Scheme,
                    NumberOfVent = numberOfVent,
                    DiagramAsImageBytes = aerodynamicPlotBytes,
                    NewMarkOfFan = nameOfFan,
                    TotalEfficiency = totalEfficiency,
                });
                rpm = MaxRPM*2;
            }

            diameter += 0.05;
        }

        return result;
    }
}




