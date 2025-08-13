namespace BladesCalc.Models;

public class DatasRightSchemes
{
    public int NomberOfScheme { get; set; }
    public double Diameter { get; set; }
    public int Rpm { get; set; }
    public required string Scheme { get; set; }
    public required byte[] DiagramAsImageBytes { get; set; }
}
