using BladesCalc.Helpers.PdfHelpers;
using BladesCalc.Models;

namespace BladesCalc.Helpers.AerodynamicHelpers;

public static class AerodinamicHelper
{       
    public static List<AerodynamicsDataBlades> GetAerodynamicByTypeBlades(List<AerodynamicsDataBlades> datas, BladesCalculationParameters parameters)
    {
        var aerodynamicsByTypeBlades = datas.Where(d => d.TypeOfBladesKod == (TypeOfBladesKodNumber)parameters.TypeOfBladesKod);

        if (aerodynamicsByTypeBlades == null)
        {
            return null;
        }

        return aerodynamicsByTypeBlades.ToList();
    }
    public static DatasRightSchemes GetRowOfRightSchemes (List<AerodynamicsDataBlades> datas, BladesCalculationParameters parameters, ParametersDrawImage parametersDrawImage)
    {
        return PaintDiagramsHelper.GenerateTableOfRightSchemes(datas, parameters, parametersDrawImage).FirstOrDefault(d => d.Scheme == parameters.RightSchemeChoose);
    }
    public static AerodynamicsDataBlades GetAerodynamicByTypeBladesRow (List<AerodynamicsDataBlades> datas, BladesCalculationParameters parameters)
    {
        return GetAerodynamicByTypeBlades(datas, parameters).FirstOrDefault(d => d.Scheme == parameters.RightSchemeChoose);
    }
}
