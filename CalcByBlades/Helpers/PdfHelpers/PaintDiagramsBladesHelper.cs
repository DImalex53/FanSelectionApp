using BladesCalc.Models;
using BladesCalc.Helpers.AerodynamicHelpers;

namespace BladesCalc.Helpers.PdfHelpers;
public static class PaintDiagramsHelper
{
    private const int MaxRPM = 3000;
    private const double MinDiameter = 0.4;
    private const double MaxDiameter = 3;
    public static List<DatasRightSchemes> GenerateTableOfRightSchemes(List<AerodynamicsDataBlades> datas, BladesCalculationParameters parameters, ParametersDrawImage parametersDrawImage)
    {
        var tableOfRightSchemes = new List<DatasRightSchemes>();
        var aerodynamicsByTypeBlades = AerodinamicHelper.GetAerodynamicByTypeBlades(datas, parameters);

        if (aerodynamicsByTypeBlades == null)
        {
            return null;
        }

        var width = parametersDrawImage.Width;
        var height = parametersDrawImage.Height;
        var imageFormat = parametersDrawImage.ImageFormat;
        int rpm;
        int nomberOfScheme = 0;
        int maxHalfPoluces = 6;

        if (parameters.NalichieVFD == 1) { maxHalfPoluces = 61;}

        for (int nomberOfRow = 0; nomberOfRow < aerodynamicsByTypeBlades.Count; nomberOfRow++)
        {
            double diameter = MinDiameter;
            var aerodynamicRowBlades = aerodynamicsByTypeBlades[nomberOfRow];

            var staticPressure1 = aerodynamicRowBlades.StaticPressure1;
            var staticPressure2 = aerodynamicRowBlades.StaticPressure2;
            var staticPressure3 = aerodynamicRowBlades.StaticPressure3;
            var minDeltaEfficiency = aerodynamicRowBlades.MinDeltaEfficiency;
            var maxDeltaEfficiency = aerodynamicRowBlades.MaxDeltaEfficiency;
            var outletLength = aerodynamicRowBlades.OutletLength;
            var outletWidth = aerodynamicRowBlades.OutletWidth;
            var efficiency1 = aerodynamicRowBlades.Efficiency1;
            var efficiency2 = aerodynamicRowBlades.Efficiency2;
            var efficiency3 = aerodynamicRowBlades.Efficiency3;
            var efficiency4 = aerodynamicRowBlades.Efficiency4;
            var newMarkOfFan = aerodynamicRowBlades.NewMarkOfFan;
            var newMarkOfFand = aerodynamicRowBlades.NewMarkOfFand;

            var nameOfFan = aerodynamicRowBlades.NewMarkOfFan;

            if (parameters.SuctionType == 1) 
            {
                nameOfFan = aerodynamicRowBlades.NewMarkOfFand;
            }

            for (int k = 0; diameter < MaxDiameter; k++)
            {
                for (int halfPoluces = 1; halfPoluces < maxHalfPoluces; halfPoluces++)
                {
                    rpm = MaxRPM / halfPoluces;

                    var flowRateMax = CalculationDiagramHelper.GetFlowRateMaxMin(
                        aerodynamicsByTypeBlades[nomberOfRow].MaxDeltaEfficiency, 
                        diameter, 
                        rpm, 
                        parameters
                        );

                    var flowRateMin = CalculationDiagramHelper.GetFlowRateMaxMin(
                        aerodynamicsByTypeBlades[nomberOfRow].MinDeltaEfficiency, 
                        diameter, 
                        rpm, 
                        parameters
                        );

                    var totalPressure = CalculationDiagramHelper.GetPolinomTotalPressure(
                        parameters.FlowRateRequired,
                        parameters,
                        rpm,
                        staticPressure1,
                        staticPressure2,
                        staticPressure3,
                        outletLength,
                        outletWidth,
                        diameter
                        );

                    var staticPressure = CalculationDiagramHelper.GetPolinomStaticPressure(
                        parameters.FlowRateRequired, 
                        parameters, 
                        rpm, 
                        staticPressure1, 
                        staticPressure2, 
                        staticPressure3, 
                        diameter
                        );

                    bool conditionMet = false;

                    if (parameters.TypeOfPressure == 0)
                    {
                        conditionMet = parameters.FlowRateRequired > flowRateMin &&
                                      parameters.FlowRateRequired < flowRateMax &&
                                      parameters.SystemResistance < totalPressure;
                    }
                    if (parameters.TypeOfPressure == 1)
                    {
                        conditionMet = parameters.FlowRateRequired > flowRateMin &&
                                      parameters.FlowRateRequired < flowRateMax &&
                                      parameters.SystemResistance < staticPressure;
                    }

                    if (conditionMet)
                    {
                        nomberOfScheme++;
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
                                imageFormat
                                );
                        tableOfRightSchemes.Add(new DatasRightSchemes
                        {
                            NomberOfScheme = nomberOfScheme,
                            Diameter = diameter,
                            Rpm = rpm,
                            Scheme = aerodynamicsByTypeBlades[nomberOfRow].Scheme,
                            DiagramAsImageBytes = aerodynamicPlotBytes,
                        });

                        halfPoluces = maxHalfPoluces;
                    }
                }
                diameter = diameter + 0.5;
            }
        }

        return tableOfRightSchemes;
    }
}



