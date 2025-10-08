

using Common;

namespace SelectedFan;

public class SelectedFanParameters : CalculationParameters 
{
    /// <summary>
    /// Тип подбора (0 - по статическому давлению, 1 - по полному давлению)
    /// </summary>
    public int TypeOfPressure { get; set; } = 0;

    /// <summary>
    /// Диаметр
    /// </summary>
    public double Diameter { get; set; } = 1;

    /// <summary>
    /// Требумые обороты рабочего колеса, об/мин
    /// (основной)
    /// </summary>
    public int Rpm { get; set; } = 990;

    /// <summary>
    /// Выбранная схема
    /// (основной)
    /// </summary>
    public string ChooseScheme { get; set; }


}
