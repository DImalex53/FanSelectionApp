using Common;

namespace BladesCalc.Models;

public class BladesCalculationParameters : CalculationParameters
{
    /// <summary>
    /// Тип лопаток (1 - Назад загнутые, 2 - радиально оканчивающиеся, 3 - аэрофольные, 4 - прямые, отклоенные назад, 5 - вперед загнутые)
    /// </summary>
    public int TypeOfBladesKod { get; set; } = 1;

    /// <summary>
    /// Тип подбора (0 - по статическому давлению, 1 - по полному давлению)
    /// </summary>
    public int TypeOfPressure { get; set; } = 0;

    /// <summary>
    /// Выбор правильной схемы
    /// </summary>
    public required string RightSchemeChoose { get; set; }
    public new double NalichieVFD { get; set; } = 0;
    public int TypeOfChoose { get; set; } = 0;
}