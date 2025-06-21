using Common;

namespace BladesCalc.Models;

public class BladesCalculationParameters : CalculationParameters
{
    public int TypeOfBladesKod { get; set; } = 1;
    public int TypeOfPressure { get; set; } = 0;
    public string RightSchemeChoose { get; set; }
    public double NalichieVFD { get; set; } = 0;
    public int TypeOfChoose { get; set; } = 0;
}