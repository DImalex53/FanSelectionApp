using BladesCalc.Helpers.PdfHelpers;
using BladesCalc.Models;
using MathNet.Numerics;

namespace BladesCalc.Helpers.AerodynamicHelpers;

public static class AerodinamicHelper
{       
    public static List<AerodynamicsDataBlades> GetAerodynamicByTypeBlades
        (
        List<AerodynamicsDataBlades> datas, 
        BladesCalculationParameters parameters
        )
    {
        var aerodynamicsByTypeBlades = datas.Where(d => d.TypeOfBladesKod == (TypeOfBladesKodNumber)parameters.TypeOfBladesKod);

        if (aerodynamicsByTypeBlades == null)
        {
            return null;
        }

        return aerodynamicsByTypeBlades.ToList();
    }
    public static DatasRightVents GetRowOfRightVent 
        (
        List<AerodynamicsDataBlades> datas, 
        BladesCalculationParameters parameters, 
        ParametersDrawImage parametersDrawImage
        )
    {
        return PaintDiagramsHelper.GenerateTableOfRightVents(datas, parameters, parametersDrawImage).FirstOrDefault(d => d.NumberOfVent == parameters.NumberOfGraph);
    }
    public static AerodynamicsDataBlades GetAerodynamicByTypeBladesRow 
        (
        List<AerodynamicsDataBlades> datas, 
        BladesCalculationParameters parameters, 
        ParametersDrawImage parametersDrawImage, 
        DatasRightVents rowVent
        )

    {
        return datas.FirstOrDefault(d => d.Scheme == rowVent.Scheme);
    }
}
