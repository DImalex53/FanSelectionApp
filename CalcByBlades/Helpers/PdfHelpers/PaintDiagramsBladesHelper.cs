using BladesCalc.Models;

namespace BladesCalc.Helpers.PdfHelpers;
public static class PaintDiagramsHelper
{
    private const int MaxRPM = 3000;
    private const double MinDiameter = 0.4;
    private const double MaxDiameter = 3;

    public static List<DatasRightSchemes> GenerateTableOfRightSchemes(List<AerodynamicsDataBlades> aerodynamicsByTypeBlades, BladesCalculationParameters parameters, ParametersDrawImage parametersDrawImage)
    {
        var resultRightSchemes = new List<DatasRightSchemes>();

        foreach (var aerodynamicRow in aerodynamicsByTypeBlades)
        {
            try
            {

                var rightSchemes = GetRightSchemes(aerodynamicRow, parameters, parametersDrawImage);

                if (rightSchemes.Any())
                {
                    resultRightSchemes.AddRange(rightSchemes);
                }
            }
            catch (Exception ex)
            {

            }
        }

        return resultRightSchemes;
    }

    private static List<DatasRightSchemes> GetRightSchemes(AerodynamicsDataBlades aerodynamicRow, BladesCalculationParameters parameters, ParametersDrawImage parametersDrawImage)
    {
        var result = new List<DatasRightSchemes>();
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
        int rpm;
        int maxHalfPoluces = parameters.NalichieVFD == 1 ? 61 : 6;

        for (int k = 0; diameter < MaxDiameter; k++)
        {
            for (int halfPoluces = maxHalfPoluces; halfPoluces > 0; halfPoluces--)
            {
                rpm = MaxRPM / halfPoluces;

                var pressure = parameters.TypeOfPressure == 0
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

                result.Add(new DatasRightSchemes
                {
                    Diameter = diameter,
                    Rpm = rpm,
                    Scheme = aerodynamicRow.Scheme,
                    DiagramAsImageBytes = aerodynamicPlotBytes,
                });

                maxHalfPoluces--;
            }

            diameter += 0.05;
        }

        return result;
    }
}




