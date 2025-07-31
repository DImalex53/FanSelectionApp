using Common;

namespace SpeedCalc.Models;

public class SpeedCalculationParameters : CalculationParameters
{
    /// <summary>
    /// Требумые обороты рабочего колеса, об/мин
    /// (основной)
    /// </summary>
    public int Rpm { get; set; } = 990;
}