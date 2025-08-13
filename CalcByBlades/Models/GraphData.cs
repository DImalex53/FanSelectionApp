namespace BladesCalc.Models;

public class GraphData
{
    public int GraphId { get; set; }
    public string GraphName { get; set; } = string.Empty;
    public byte[] GraphImage { get; set; } = Array.Empty<byte>();
    public double Efficiency { get; set; }
    public double StaticPressure { get; set; }
    public double Diameter { get; set; }
    public int Rpm { get; set; }
    public string FanMark { get; set; } = string.Empty;
    public string FanMarkD { get; set; } = string.Empty;
    public double ImpellerWidth { get; set; }
    public double BladeWidth { get; set; }
    public double BladeLength { get; set; }
    public int NumberOfBlades { get; set; }
    public double ImpellerInletDiameter { get; set; }
    public string TypeOfBlades { get; set; } = string.Empty;
}
