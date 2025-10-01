namespace BladesCalc.Models;

    public class DatasRightVents
    {
        public int NumberOfVent { get; set; }
        public double Diameter { get; set; }
        public int Rpm { get; set; }
        public required string Scheme { get; set; }
        public byte[] DiagramAsImageBytes { get; set; } = [];
        public string NewMarkOfFan { get; set; }
        public double TotalEfficiency { get; set; }
    }
