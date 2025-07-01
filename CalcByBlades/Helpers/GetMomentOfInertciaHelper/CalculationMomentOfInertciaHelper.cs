using BladesCalc.Helpers.AerodynamicHelpers;
using BladesCalc.Models;

namespace BladesCalc.Helpers.GetMomentOfInertciaHelper;

public static class CalculationMomentOfInertciaHelper
{
    private readonly static double Stock = 1.15;
    private readonly static double StockOfHub = 1.15;
    public static double GetMomentOfInertcia(
        List<AerodynamicsDataBlades> datas, 
        BladesCalculationParameters parameters, 
        ParametersDrawImage parametersDrawImage,
        double impellerWidth, 
        double bladeWidth, 
        double bladeLength, 
        double numberOfBlades, 
        double impellerInletDiameter,
        double diameter
        )
    {
        double Coefficient = 8;

        double bladeThickness;
        double coverDiskThickness;
        double mainDiskThickness;

        GetThicknesses(diameter, out bladeThickness, out mainDiskThickness, out coverDiskThickness, datas, parameters, parametersDrawImage);

        double momentOfInertcia = (GetWeightOfMainDisk(diameter,
        mainDiskThickness, parameters) * Math.Pow(diameter, 2) / Coefficient * StockOfHub + (GetWeightOfBlade(
        bladeLength, bladeWidth, diameter, bladeThickness, parameters) * numberOfBlades + GetWeightOfCoverDisk(
        diameter, impellerWidth, coverDiskThickness, impellerInletDiameter, parameters)) * (Math.Pow(diameter, 2) +
        Math.Pow(impellerInletDiameter * diameter, 2)) / Coefficient) * Stock;

        momentOfInertcia = parameters.SuctionType == 1 ? momentOfInertcia * 2 : momentOfInertcia;

        if (parameters.SuctionType == 1)
        {
            momentOfInertcia = momentOfInertcia * 2;
        }

        return momentOfInertcia;
    }
    public static double GetWeightOfImpeller(double bladeLength, double bladeWidth, double diameter,
        double bladeThickness, int numberOfBlades, double mainDiskThickness, double impellerWidth,
        double coverDiskThickness, double impellerInletDiameter, BladesCalculationParameters parameters)
    {
        double weightOfImpeller = (GetWeightOfBlade(bladeLength, bladeWidth, diameter,
        bladeThickness, parameters) * numberOfBlades + GetWeightOfMainDisk(diameter,
        mainDiskThickness, parameters) * StockOfHub + GetWeightOfCoverDisk(diameter, impellerWidth,
        coverDiskThickness, impellerInletDiameter, parameters)) * Stock;

        if (parameters.SuctionType == 1)
        {
            weightOfImpeller = weightOfImpeller * 2;
        }

        return weightOfImpeller;
    }
    private static double GetWeightOfBlade(double bladeLength, double bladeWidth, double diameter,
        double bladeThickness, BladesCalculationParameters parameters)
    {
        double weightOfBlade = parameters.MaterialDensyti * bladeLength * bladeWidth * Math.Pow(diameter, 2) * bladeThickness;

        return weightOfBlade;
    }
    private static double GetWeightOfMainDisk(double diameter,
        double mainDiskThickness, BladesCalculationParameters parameters)
    {
        int Coefficient = 4;

        double weightOfMainDisk = parameters.MaterialDensyti * double.Pi * Math.Pow(diameter, 2) * mainDiskThickness / Coefficient;

        return weightOfMainDisk;
    }
    private static double GetWeightOfCoverDisk(double diameter, double impellerWidth,
        double coverDiskThickness, double impellerInletDiameter, BladesCalculationParameters parameters)
    {
        int Coefficient = 2;
        double coverDiskGeneratrix = GetCoverDiskGeneratrix(impellerWidth, diameter, impellerInletDiameter);

        double weightOfCoverDisk = parameters.MaterialDensyti * double.Pi * (diameter + impellerInletDiameter) *
            coverDiskGeneratrix * coverDiskThickness / Coefficient;

        return weightOfCoverDisk;
    }
    private static double GetCoverDiskGeneratrix(double impellerWidth, double diameter, double impellerInletDiameter)
    {
        double coverDiskGeneratrix = Math.Pow(Math.Pow(impellerWidth, 2) + Math.Pow(diameter * (1 - impellerInletDiameter), 2),
            0.5);

        return coverDiskGeneratrix;
    }
    private static void GetThicknesses(
        double diameter,
        out double bladeThickness,
        out double mainDiskThickness,
        out double coverDiskThickness,
        List<AerodynamicsDataBlades> datas,
        BladesCalculationParameters parameters,
        ParametersDrawImage parametersDrawImage)
    {
        bladeThickness = 0;
        mainDiskThickness = 0;
        coverDiskThickness = 0;

        DatasRightSchemes rowOfRightSchemes = AerodinamicHelper.GetRowOfRightSchemes(datas, parameters, parametersDrawImage);

        if (diameter <= 1.00 && rowOfRightSchemes.Rpm <= 1500)
        {
            bladeThickness = 0.003;
            mainDiskThickness = 0.005;
            coverDiskThickness = 0.003;
        }
        if (1.00 < diameter && diameter <= 1.50 && rowOfRightSchemes.Rpm <= 1500)
        {
            bladeThickness = 0.005;
            mainDiskThickness = 0.008;
            coverDiskThickness = 0.005;
        }
        if (1.00 < diameter && diameter <= 1.50 && rowOfRightSchemes.Rpm > 1500)
        {
            bladeThickness = 0.01;
            mainDiskThickness = 0.014;
            coverDiskThickness = 0.01;
        }
        if (1.50 < diameter && diameter <= 2.00 && 750 < rowOfRightSchemes.Rpm && rowOfRightSchemes.Rpm <= 1500)
        {
            bladeThickness = 0.008;
            mainDiskThickness = 0.01;
            coverDiskThickness = 0.08;
        }
        if (2.00 < diameter && rowOfRightSchemes.Rpm <= 1500)
        {
            bladeThickness = 0.012;
            mainDiskThickness = 0.02;
            coverDiskThickness = 0.012;
        }
        if (diameter < 1.00 && rowOfRightSchemes.Rpm > 1500)
        {
            bladeThickness = 0.005;
            mainDiskThickness = 0.008;
            coverDiskThickness = 0.005;
        }
        if (1.50 < diameter && diameter <= 2.00 && rowOfRightSchemes.Rpm > 1500)
        {
            bladeThickness = 0.012;
            mainDiskThickness = 0.02;
            coverDiskThickness = 0.012;
        }
        if (1.50 < diameter && diameter <= 2.00 && rowOfRightSchemes.Rpm <= 750)
        {
            bladeThickness = 0.005;
            mainDiskThickness = 0.005;
            coverDiskThickness = 0.005;
        }

    }
}
