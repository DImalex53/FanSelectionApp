namespace Common;

public class CalculationParameters
{
    /// <summary>
    /// Заданная производительность в рабочей точке, м3/ч
    /// (основной)
    /// </summary>
    public double FlowRateRequired { get; set; } = 100000;

    /// <summary>
    /// Требуемое давление, Па
    /// (основной)
    /// </summary>
    public double SystemResistance { get; set; } = 6000;

    /// <summary>
    /// Плотность перемещаемой среды в рабочей точке, кг/м3
    /// (основной)
    /// </summary>
    public double Density { get; set; } = 1.204;

    /// <summary>
    /// Тип перемещаемой среды
    /// (основной)
    /// </summary>
    public int Type { get; set; } = 2;

    /// <summary>
    /// Тип всасывания (односторонний, двусторонний)
    /// (основной)
    /// </summary>
    public int SuctionType { get; set; } = 0;

    /// <summary>
    /// Напряжение сети, В
    /// </summary>
    public double MotorVoltage { get; set; } = 380;

    /// <summary>
    /// Плотность среды во время пуска, кг/м3
    /// </summary>
    public double StartInletDensity { get; set; } = 1.204;

    /// <summary>
    /// Плотность материала изготовления рабочего колеса, кг/м3
    /// </summary>
    public double MaterialDensyti { get; set; } = 8000;

    /// <summary>
    /// Номер задачи в Bitrix24
    /// </summary>
    public int? NumberOfTask { get; set; } = null;

    /// <summary>
    /// Схема конструктивного исполнения
    /// </summary>
    public int? ConstructScheme { get; set; } = null;

    /// <summary>
    /// Направление вращения (правое, левое)
    /// </summary>
    public int? RotaitionDirection { get; set; } = 0;

    /// <summary>
    /// Угол разворота корпуса, градусы
    /// </summary>
    public int? ExhaustDirection { get; set; } = null;

    /// <summary>
    /// Виброизоляция (не требуется, требуется)
    /// </summary>
    public bool Vibroisolation { get; set; } = false;

    /// <summary>
    /// Направляющий аппарат (не требуется, требуется)
    /// </summary>
    public bool GuideVane { get; set; } = false;

    /// <summary>
    /// Теплошумоизолирующий кожух (не требуется, требуется)
    /// </summary>
    public bool Teploisolation { get; set; } = false;

    /// <summary>
    /// Материальное исполнение ТДМ (K1, B, BK1, Ti)
    /// </summary>
    public int MaterialDesign { get; set; } = 5;

    /// <summary>
    /// Материал изготовления рабочего колеса
    /// </summary>
    public string MaterialOfImpeller { get; set; } = "09Г2С";

    /// <summary>
    /// Материал изготовления корпуса улиты
    /// </summary>
    public string MaterialOfUlita { get; set; } = "09Г2С";

    /// <summary>
    /// Материал изготовления корпуса крепежа улиты
    /// </summary>
    public string? MaterialOfBoltsOfUlita { get; set; } = null;

    /// <summary>
    /// Материал изготовления рамы
    /// </summary>
    public string MaterialOfRama { get; set; } = "09Г2С";

    /// <summary>
    /// Тип муфты
    /// </summary>
    public string? MuftType { get; set; } = null;

    /// <summary>
    /// Тип ходовой части
    /// </summary>
    public string? TypeOfPPO { get; set; } = null;

    /// <summary>
    /// Климатическое исполнение
    /// </summary>
    public string? Klimatic { get; set; } = null;

    /// <summary>
    /// Дополнительные требования к электродвигателю
    /// </summary>
    public string? DopTrebovaniyaMotor { get; set; } = null;

    /// <summary>
    /// Тип уплотенения вала в районе корпуса/кармана
    /// </summary>
    public string? ShaftSeal { get; set; } = null;

    /// <summary>
    /// Тип компенсатора на входе
    /// </summary>
    public string? TypeOfCompensatorInlet { get; set; } = null;

    /// <summary>
    /// Тип компенсатора на выходе
    /// </summary>
    public string? TypeOfCompensatorOutlet { get; set; } = null;

    /// <summary>
    /// Входной фланец (не предусмотрен, предусмотрен)
    /// </summary>
    public bool FlangeOutlet { get; set; } = false;

    /// <summary>
    /// Выходной фланец (не предусмотрен, предусмотрен)
    /// </summary>
    public bool FlangeInlet { get; set; } = false;

    /// <summary>
    /// Датчики вибрации ходовой части
    /// </summary>
    public string? VibroSensorPPO { get; set; } = null;

    /// <summary>
    /// Датчики температуры ходовой части
    /// </summary>
    public string? TempSensorPPO { get; set; } = null;

    /// <summary>
    /// Датчики вибрации электродвигателя
    /// </summary>
    public string? VibroSensorMotor { get; set; } = null;

    /// <summary>
    /// Маркировка взрывозащиты
    /// </summary>
    public string? MarkOfVzrivMotor { get; set; } = null;

    /// <summary>
    /// Наличие преобразователя частоты (не предусмотрен, предусмотрен)
    /// </summary>
    public bool NalichieVFD { get; set; } = false;

    /// <summary>
    /// Диапазон регулирования (10..50 Гц, 10..60 Гц)
    /// </summary>
    public string? RangeOfVFD { get; set; } = null;

    /// <summary>
    /// Название проекта
    /// </summary>
    public string ProjectName { get; set; } = "Изготовление и поставка тягодутьевой машины";

    /// <summary>
    /// Дополнительная комплектация
    /// </summary>
    public string? DopKomplekt { get; set; } = null;

    /// <summary>
    /// Дополнительные требования к ТДМ
    /// </summary>
    public string? DopTrebovanyaTDM { get; set; } = null;

    /// <summary>
    /// ЗиП
    /// </summary>
    public string? Zip { get; set; }

    /// <summary>
    /// Шеф-монтаж (не предусмотрен, предусмотрен)
    /// </summary>
    public bool ShefMontage { get; set; } = false;

    /// <summary>
    /// Пуско-наладочные работы (не предусмотрены, предусмотрены)
    /// </summary>
    public bool PuskoNaladka { get; set; } = false;

    /// <summary>
    /// Обучение обслуживающего персонала (не предусмотрено, предусмотрено)
    /// </summary>
    public bool StudyOfPersonal { get; set; } = false;
}
