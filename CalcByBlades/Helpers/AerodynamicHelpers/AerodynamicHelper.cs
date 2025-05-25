using BladesCalc.Helpers.PdfHelpers;
using BladesCalc.Models;
using static iText.IO.Image.Jpeg2000ImageData;

namespace BladesCalc.Helpers.AerodynamicHelpers;

public static class AerodinamicHelper
{       
    public static List<AerodynamicsDataBlades> GetAerodynamicByTypeBlades(List<AerodynamicsDataBlades> datas, CalculationParameters parameters)
    {
        var aerodynamicsByTypeBlades = datas.Where(d => d.TypeOfBladesKod == (TypeOfBladesKodNumber)parameters.TypeOfBladesKod);

        if (aerodynamicsByTypeBlades == null)
        {
            return null;
        }

        return aerodynamicsByTypeBlades.ToList();
    }
    public static DatasRightSchemes GetRowOfRightSchemes (List<AerodynamicsDataBlades> datas, CalculationParameters parameters)
    {
        return PaintDiagramsHelper.GenerateTableOfRightSchemes(datas, parameters).FirstOrDefault(d => d.Scheme == parameters.RightSchemeChoose);
    }
    public static AerodynamicsDataBlades GetAerodynamicByTypeBladesRow (List<AerodynamicsDataBlades> datas, CalculationParameters parameters)
    {
        return GetAerodynamicByTypeBlades(datas, parameters).FirstOrDefault(d => d.Scheme == parameters.RightSchemeChoose);
    }
}
