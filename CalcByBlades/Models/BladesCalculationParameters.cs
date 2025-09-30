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
    /// Номер выбранного графика
    /// </summary> 
    public int NumberOfGraph {  get; set; } = 0;
}