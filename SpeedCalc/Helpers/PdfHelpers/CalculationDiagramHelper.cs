
using MathNet.Numerics;
using SpeedCalc.Helpers.GetDiameterHelpers;
using SpeedCalc.Models;


namespace SpeedCalc.Helpers.PdfHelpers;

public static class CalculationDiagramHelper
{
    public static (double[] flowRates, double[] pressureResistances) GetPressureResistanceMassive(
        int pointsCount,
        SpeedCalculationParameters parameters,
        double flowRateMax)
    {
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
        List<AerodynamicsData> datas,
        SpeedCalculationParameters parameters)
    {
        double RpmValueMax = parameters.Rpm;
        double RpmValueMin = 0;
        double[] rpmValues = Generate.LinearSpaced(pointsCount, RpmValueMin, RpmValueMax);
        double[] nominalTorques = new double[pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            nominalTorques[i] = GetPolinomNominalTorque(rpmValues[i], datas, parameters);
        }

        return (rpmValues, nominalTorques);
    }
    public static (double[] rpmValues, double[] torqueWithGates) GetTorqueWithGateMassive(
        int pointsCount, List<AerodynamicsData> datas,
        SpeedCalculationParameters parameters)
    {
        double RpmValueMax = parameters.Rpm;
        double RpmValueMin = 0;
        double[] rpmValues = Generate.LinearSpaced(pointsCount, RpmValueMin, RpmValueMax);
        double[] torqueWithGates = new double[pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            torqueWithGates[i] = GetPolinomNominalTorque(rpmValues[i], datas, parameters) / 3;
        }

        return (rpmValues, torqueWithGates);
    }
    public static (double[] flowRates, double[] staticPressures) GetStaticPressureMassive(
        int pointsCount, 
        SpeedCalculationParameters parameters,
        List<AerodynamicsData> datas,
        int rpm)
    {
        var aerodynamicRow = AerodinamicRowHelper.GetAerodinamicRow(datas, parameters);
        var diameter = CalculationDiameterHelper.GetDiameter(datas, parameters);

        double flowRateMin = GetFlowRateMaxMin(aerodynamicRow.MinDeltaEfficiency, diameter, parameters, rpm);
        double flowRateMax = GetFlowRateMaxMin(aerodynamicRow.MaxDeltaEfficiency, diameter, parameters, rpm);

        double[] flowRates = Generate.LinearSpaced(pointsCount, flowRateMin, flowRateMax);
        double[] staticPressures = new double[pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            staticPressures[i] = GetPolinomStaticPressure(
               flowRates[i], parameters, datas, rpm);
        }

        return (flowRates, staticPressures);
    }
    public static (double[] flowRates, double[] totalPressures) GetTotalPressureMassive(
        int pointsCount, SpeedCalculationParameters parameters,
        List<AerodynamicsData> datas,
        int rpm)
    {
        var aerodynamicRow = AerodinamicRowHelper.GetAerodinamicRow(datas, parameters);
        var diameter = CalculationDiameterHelper.GetDiameter(datas, parameters);

        double flowRateMin = GetFlowRateMaxMin(aerodynamicRow.MinDeltaEfficiency, diameter, parameters, rpm);
        double flowRateMax = GetFlowRateMaxMin(aerodynamicRow.MaxDeltaEfficiency, diameter, parameters, rpm);

        double[] flowRates = Generate.LinearSpaced(pointsCount, flowRateMin, flowRateMax);
        double[] totalPressures = new double[pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            totalPressures[i] = GetPolinomTotalPressure(
                flowRates[i], datas, parameters, rpm);
        }

        return (flowRates, totalPressures);
    }
    public static (double[] flowRates, double[] powers) GetPowerMassive(
        int pointsCount,
        SpeedCalculationParameters parameters,
        List<AerodynamicsData> datas,
        int rpm)
    {
        var aerodynamicRow = AerodinamicRowHelper.GetAerodinamicRow(datas, parameters);
        var diameter = CalculationDiameterHelper.GetDiameter(datas, parameters);

        double flowRateMin = GetFlowRateMaxMin(aerodynamicRow.MinDeltaEfficiency, diameter, parameters, rpm);
        double flowRateMax = GetFlowRateMaxMin(aerodynamicRow.MaxDeltaEfficiency, diameter, parameters, rpm);
        double CoefficientStock = 1;

        double[] flowRates = Generate.LinearSpaced(pointsCount, flowRateMin, flowRateMax);
        double[] powers = new double[pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            powers[i] = GetPolinomPower(
                flowRates[i],
                datas, parameters, rpm);
        }

        return (flowRates, powers);
    }
    private static double GetDinamicPressure(double flowRate, SpeedCalculationParameters parameters, List<AerodynamicsData> datas)
    {
        var aerodynamicRow = AerodinamicRowHelper.GetAerodinamicRow(datas, parameters);
        var diameter = CalculationDiameterHelper.GetDiameter(datas, parameters);
        var outletLength = aerodynamicRow.OutletLength;

        if (parameters.SuctionType == 1)
        {
            outletLength = outletLength * 2;
        }

        int SecondsInHour = 3600;
        int Coefficient = 2;

        return parameters.Density * Math.Pow(flowRate / (SecondsInHour * outletLength * aerodynamicRow.OutletWidth
             * Math.Pow(diameter, 2)), 2) / Coefficient;
    }
    private static double GetPolinomPressureResistance(double flowRate, SpeedCalculationParameters parameters)
    {
        return parameters.SystemResistance * Math.Pow(flowRate / parameters.FlowRateRequired, 2);
    }
    public static double GetPolinomStaticPressure(
        double flowRate, 
        SpeedCalculationParameters parameters, 
        List<AerodynamicsData> datas, 
        int rpm)
    {
        int Coefficient = 2;
        int SecondsInHour = 3600;
        int Coefficient1 = parameters.SuctionType == 1 ? 2 : 1;

        var aerodynamicRow = AerodinamicRowHelper.GetAerodinamicRow(datas, parameters);
        var diameter = CalculationDiameterHelper.GetDiameter(datas, parameters);

        var peripheralSpeed = GetPeripheralSpeed(diameter, parameters, rpm);
        var areaImpeller = GetAreaImpeller(diameter);

        return aerodynamicRow.StaticPressure1 * Math.Pow(flowRate / Coefficient1 / areaImpeller / SecondsInHour, 2) *
            parameters.Density / Coefficient + aerodynamicRow.StaticPressure2 * parameters.Density * peripheralSpeed *
            flowRate / Coefficient1 / areaImpeller / SecondsInHour /
            Coefficient + aerodynamicRow.StaticPressure3 * parameters.Density / Coefficient * Math.Pow(peripheralSpeed, 2);
    }
    public static double GetPolinomTotalPressure(
        double flowRate, 
        List<AerodynamicsData> datas, 
        SpeedCalculationParameters parameters, 
        int rpm)
    {
        return GetPolinomStaticPressure(flowRate, parameters, datas, rpm) + GetDinamicPressure(flowRate, parameters, datas);
    }
    public static double GetPolinomPower(
        double flowRate, 
        List<AerodynamicsData> datas, 
        SpeedCalculationParameters parameters, 
        int rpm)
    {
        int Coefficient = 1000;
        int SecondsInHour = 3600;

        return flowRate * GetPolinomTotalPressure(flowRate, datas, parameters, rpm) /
            GetPolinomEeficiency(flowRate, datas, parameters, rpm) / Coefficient / SecondsInHour;
    }
    public static double GetPolinomEeficiency(
        double flowRate, 
        List<AerodynamicsData> datas, 
        SpeedCalculationParameters parameters, 
        int rpm)
    {
        var aerodynamicRow = AerodinamicRowHelper.GetAerodinamicRow(datas, parameters);
        var diameter = CalculationDiameterHelper.GetDiameter(datas, parameters);

        double peripheralSpeed = GetPeripheralSpeed(diameter, parameters, rpm);
        double areaImpeller = GetAreaImpeller(diameter);
        int SecondsInHour = 3600;
        int Coefficient = parameters.SuctionType == 1 ? 2 : 1;

        return aerodynamicRow.Efficiency1 * Math.Pow(flowRate / Coefficient / peripheralSpeed / areaImpeller / SecondsInHour,
            3) + aerodynamicRow.Efficiency2 * Math.Pow(flowRate / Coefficient / peripheralSpeed / areaImpeller / SecondsInHour, 2) +
            aerodynamicRow.Efficiency3 * flowRate / Coefficient / peripheralSpeed / areaImpeller / SecondsInHour + aerodynamicRow.Efficiency4;
    }

    public static double GetPolinomStaticEficiency(
        double flowRate, 
        List<AerodynamicsData> datas, 
        SpeedCalculationParameters parameters, 
        int rpm)
    {
        int SecondsInHour = 3600;
        int Coefficient = 1000;

        return GetPolinomStaticPressure(flowRate, parameters, datas, rpm) * flowRate / SecondsInHour / Coefficient /
        GetPolinomPower(flowRate, datas, parameters, rpm);
    }
    private static double GetPeripheralSpeed(
        double diameter, 
        SpeedCalculationParameters parameters, 
        int rpm)
    {
        int SecPerMinute = 60;
        return double.Pi * diameter * rpm / SecPerMinute;
    }
    private static double GetAreaImpeller(double diameter)
    {
        int Coefficient = 4;
        return double.Pi * Math.Pow(diameter, 2) / Coefficient;
    }
    private static double GetFlowRateMaxMin(
        double deltaEfficiency, 
        double diameter, 
        SpeedCalculationParameters parameters, 
        int rpm)
    {
        double peripheralSpeed = GetPeripheralSpeed(diameter, parameters, rpm);
        double areaImpeller = GetAreaImpeller(diameter);
        int SecondsInHour = 3600;
        int Coefficient = 1;

        if (parameters.SuctionType == 1)
        {
            Coefficient = 2;
        }

        return deltaEfficiency * SecondsInHour * peripheralSpeed * areaImpeller * Coefficient;
    }
    private static double GetPolinomNominalTorque(double rpmValue, List<AerodynamicsData> datas, SpeedCalculationParameters parameters)
    {
        int SecPerMinute = 60;

        return GetPolinomPower(parameters.FlowRateRequired, datas, parameters, parameters.Rpm) * SecPerMinute / parameters.Rpm
            * Math.Pow(rpmValue / parameters.Rpm, 2);
    }
}

