using System.Drawing;

namespace src.Data;

public class Block
{
    public Block(List<string> patientData)
    {
        PatientData = patientData;
    }
    public Block() { }
    public List<Nplicate> Nplicates { get; set; } = new List<Nplicate>();
    public List<string> PatientData { get; set; } = new List<string>();
    public Color TextColour { get; set; } = new Color();
    public double QC { get; set; } = 0;

    public void CalculateQC(Nplicate pos, Nplicate neg)
    {
        QC = pos.Mean == 0 ? double.NaN : (pos.Mean - neg.Mean) / pos.Mean;
    }
}