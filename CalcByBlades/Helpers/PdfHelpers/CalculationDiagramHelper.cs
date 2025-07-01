using MathNet.Numerics;
using BladesCalc.Models;


namespace BladesCalc.Helpers.PdfHelpers;

public static class CalculationDiagramHelper
{
    public static (double[] flowRates, double[] pressureResistances) GetPressureResistanceMassive(
        int pointsCount,
        double flowRateWorkPoint,
        BladesCalculationParameters parameters)
    {
        double flowRateMax = flowRateWorkPoint;
        double FlowRateMin = 0;
        double[] flowRates = Generate.LinearSpaced(pointsCount, FlowRateMin, flowRateMax);
        double[] pressureResistances = new double[pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            pressureResistances[i] = GetPolinomPressureResistance(flowRates[i], parameters);
        }

        return (flowRates, pressureResistances);
    }
    public static (double[] rpmValues, double[] nominalTorques) GetNominalTorqueMassive(
        int pointsCount,
        double flowRateWorkPoint,
        BladesCalculationParameters parameters,
        int rpm,
        double staticPressure1,
        double staticPressure2,
        double staticPressure3,
        double outletLength,
        double outletWidth,
        double efficiency1,
        double efficiency2,
        double efficiency3,
        double efficiency4,
        double diameter
        )
    {
        double RpmValueMax = rpm;
        double RpmValueMin = 0;
        double[] rpmValues = Generate.LinearSpaced(pointsCount, RpmValueMin, RpmValueMax);
        double[] nominalTorques = new double[pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            nominalTorques[i] = GetPolinomNominalTorque(
                rpmValues[i],
                flowRateWorkPoint,
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
        }

        return (rpmValues, nominalTorques);
    }
    public static (double[] rpmValues, double[] torqueWithGates) GetTorqueWithGateMassive(
        int pointsCount, 
        double flowRateWorkPoint,
        BladesCalculationParameters parameters,
        int rpm,
        double staticPressure1,
        double staticPressure2,
        double staticPressure3,
        double outletLength,
        double outletWidth,
        double efficiency1,
        double efficiency2,
        double efficiency3,
        double efficiency4,
        double diameter
        )
    {
        double RpmValueMax = rpm;
        double RpmValueMin = 0;
        double[] rpmValues = Generate.LinearSpaced(pointsCount, RpmValueMin, RpmValueMax);
        double[] torqueWithGates = new double[pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            torqueWithGates[i] = GetPolinomNominalTorque(
                rpmValues[i],
                flowRateWorkPoint,
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
        ) / 3;
        }

        return (rpmValues, torqueWithGates);
    }
    public static (double[] flowRates, double[] staticPressures) GetStaticPressureMassive(
        int pointsCount,
        BladesCalculationParameters parameters,
        int rpm,
        double staticPressure1,
        double staticPressure2,
        double staticPressure3,
        double minDeltaEfficiency,
        double maxDeltaEfficiency,
        double diameter
        )
    {
        double flowRateMin = GetFlowRateMaxMin(minDeltaEfficiency, diameter, rpm, parameters);
        double flowRateMax = GetFlowRateMaxMin(maxDeltaEfficiency, diameter, rpm, parameters);

        double[] flowRates = Generate.LinearSpaced(pointsCount, flowRateMin, flowRateMax);
        double[] staticPressures = new double[pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            staticPressures[i] = GetPolinomStaticPressure(
               flowRates[i], 
               parameters, 
               rpm,
               staticPressure1,
               staticPressure2,
               staticPressure3,
               diameter);
        }

        return (flowRates, staticPressures);
    }
    public static (double[] flowRates, double[] totalPressures) GetTotalPressureMassive(
        int pointsCount,
        BladesCalculationParameters parameters,
        int rpm,
        double staticPressure1,
        double staticPressure2,
        double staticPressure3,
        double outletLength,
        double outletWidth,
        double minDeltaEfficiency,
        double maxDeltaEfficiency,
        double diameter)
    {      
        double flowRateMin = GetFlowRateMaxMin(minDeltaEfficiency, diameter, rpm, parameters);
        double flowRateMax = GetFlowRateMaxMin(maxDeltaEfficiency, diameter, rpm, parameters);

        double[] flowRates = Generate.LinearSpaced(pointsCount, flowRateMin, flowRateMax);
        double[] totalPressures = new double[pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            totalPressures[i] = GetPolinomTotalPressure(
                flowRates[i], parameters,
                rpm,
                staticPressure1,
                staticPressure2,
                staticPressure3,
                outletLength,
                outletWidth,
                diameter);
        }

        return (flowRates, totalPressures);
    }
    public static (double[] flowRates, double[] powers) GetPowerMassive(
        int pointsCount,
        BladesCalculationParameters parameters,
        int rpm,
        double staticPressure1,
        double staticPressure2,
        double staticPressure3,
        double outletLength,
        double outletWidth,
        double efficiency1,
        double efficiency2,
        double efficiency3,
        double efficiency4,
        double minDeltaEfficiency,
        double maxDeltaEfficiency,
        double diameter)
    {
        double flowRateMin = GetFlowRateMaxMin(minDeltaEfficiency, diameter, rpm, parameters);
        double flowRateMax = GetFlowRateMaxMin(maxDeltaEfficiency, diameter, rpm, parameters);
        double CoefficientStock = 1;

        double[] flowRates = Generate.LinearSpaced(pointsCount, flowRateMin, flowRateMax);
        double[] powers = new double[pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            powers[i] = GetPolinomPower(
                flowRates[i],
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
        }

        return (flowRates, powers);
    }
    private static double GetDinamicPressure(
        double flowRate,
        BladesCalculationParameters parameters,  
        double outletLength,
        double outletWidth,
        double diameter
        )
    {
        outletLength = parameters.SuctionType == 1 ? outletLength * 2 : outletLength;

        int SecondsInHour = 3600;
        int Coefficient = 2;

        return parameters.Density * Math.Pow(flowRate / (SecondsInHour * outletLength * outletWidth
             * Math.Pow(diameter, 2)), 2) / Coefficient;
    }
    private static double GetPolinomPressureResistance(double flowRate, BladesCalculationParameters parameters)
    {
        double polinomPresureResistence = parameters.SystemResistance * Math.Pow(flowRate / parameters.FlowRateRequired, 2);

        return polinomPresureResistence;
    }
    public static double GetPolinomStaticPressure(
        double flowRate,
        BladesCalculationParameters parameters, 
        int rpm, 
        double staticPressure1, 
        double staticPressure2, 
        double staticPressure3,
        double diameter)
    {
        int Coefficient = 2;
        int SecondsInHour = 3600;
        int Coefficient1 = parameters.SuctionType == 1 ? 2 : 1;

        var peripheralSpeed = GetPeripheralSpeed(diameter, rpm);
        var areaImpeller = GetAreaImpeller(diameter);

        var polinomStaticPressure = staticPressure1 * Math.Pow(flowRate / Coefficient1 / areaImpeller / SecondsInHour, 2) *
            parameters.Density / Coefficient + staticPressure2 * parameters.Density * peripheralSpeed *
            flowRate / Coefficient1 / areaImpeller / SecondsInHour /
            Coefficient + staticPressure3 * parameters.Density / Coefficient * Math.Pow(peripheralSpeed, 2);

        return polinomStaticPressure;
    }
    public static double GetPolinomTotalPressure(
        double flowRate,
        BladesCalculationParameters parameters,
        int rpm,
        double staticPressure1,
        double staticPressure2,
        double staticPressure3,
        double outletLength,
        double outletWidth,
        double diameter)
    {
        return GetPolinomStaticPressure(flowRate, parameters, rpm, staticPressure1, staticPressure2, staticPressure3, diameter) + 
            GetDinamicPressure(flowRate, parameters, outletLength, outletWidth, diameter);
    }
    public static double GetPolinomPower(
        double flowRate,
        BladesCalculationParameters parameters, 
        int rpm,
        double staticPressure1,
        double staticPressure2,
        double staticPressure3,
        double outletLength,
        double outletWidth,
        double efficiency1,
        double efficiency2,
        double efficiency3,
        double efficiency4,
        double diameter)
    {
        int Coefficient = 1000;
        int SecondsInHour = 3600;

        return flowRate * GetPolinomTotalPressure(flowRate, parameters, rpm, staticPressure1, staticPressure2, staticPressure3, outletLength, outletWidth, diameter) /
            GetPolinomTotalEeficiency(flowRate, parameters, rpm, efficiency1, efficiency2, efficiency3, efficiency4, diameter) / Coefficient / SecondsInHour;
    }
    public static double GetPolinomTotalEeficiency(
        double flowRate,
        BladesCalculationParameters parameters,
        int rpm,
        double efficiency1,
        double efficiency2,
        double efficiency3,
        double efficiency4,
        double diameter)
    {
        double peripheralSpeed = GetPeripheralSpeed(diameter, rpm);
        double areaImpeller = GetAreaImpeller(diameter);
        int SecondsInHour = 3600;
        int Coefficient = parameters.SuctionType == 1 ? 2 : 1;

        return efficiency1 * Math.Pow(flowRate / Coefficient / peripheralSpeed / areaImpeller / SecondsInHour,
            3) + efficiency2 * Math.Pow(flowRate / Coefficient / peripheralSpeed / areaImpeller / SecondsInHour, 2) +
            efficiency3 * flowRate / Coefficient / peripheralSpeed / areaImpeller / SecondsInHour + efficiency4;
    }

    public static double GetPolinomStaticEficiency(
        double flowRate,
        BladesCalculationParameters parameters,
        int rpm,
        double staticPressure1,
        double staticPressure2,
        double staticPressure3,
        double outletLength,
        double outletWidth,
        double efficiency1,
        double efficiency2,
        double efficiency3,
        double efficiency4,
        double diameter)
    {
        int SecondsInHour = 3600;
        int Coefficient = 1000;

        return GetPolinomStaticPressure(flowRate, parameters, rpm, staticPressure1, staticPressure2, staticPressure3, diameter) * flowRate / SecondsInHour / Coefficient /
        GetPolinomPower(flowRate, parameters, rpm, staticPressure1, staticPressure2, staticPressure3, outletLength, outletWidth, efficiency1, efficiency2, efficiency3, efficiency4, diameter);
    }
    private static double GetPeripheralSpeed(
        double diameter, 
        int rpm)
    {
        int SecPerMinute = 60;
        var peripheralSpeed = double.Pi * diameter * rpm / SecPerMinute;

        return peripheralSpeed;
    }
    private static double GetAreaImpeller(double diameter)
    {
        int Coefficient = 4;
        return double.Pi * Math.Pow(diameter, 2) / Coefficient;
    }
    public static double GetFlowRateMaxMin(
        double deltaEfficiency, 
        double diameter, 
        int rpm,
        BladesCalculationParameters parameters)
    {
        double peripheralSpeed = GetPeripheralSpeed(diameter, rpm);
        double areaImpeller = GetAreaImpeller(diameter);
        int SecondsInHour = 3600;
        int Coefficient = parameters.SuctionType == 1 ? 2 : 1;

        return deltaEfficiency * SecondsInHour * peripheralSpeed * areaImpeller * Coefficient;
    }
    private static double GetPolinomNominalTorque(
        double rpmValue,
        double flowRateWorkPoint,
        BladesCalculationParameters parameters,
        int rpm,
        double staticPressure1,
        double staticPressure2,
        double staticPressure3,
        double outletLength,
        double outletWidth,
        double efficiency1,
        double efficiency2,
        double efficiency3,
        double efficiency4,
        double diameter
        )
    {
        int SecPerMinute = 60;

        return GetPolinomPower(flowRateWorkPoint, parameters, rpm, staticPressure1, staticPressure2, staticPressure3, outletLength, outletWidth, 
            efficiency1, efficiency2, efficiency3, efficiency4, diameter) * SecPerMinute / rpm
            * Math.Pow(rpmValue / rpm, 2);
    }
}

