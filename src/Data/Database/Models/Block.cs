using System.Drawing;

namespace src.Data;

public class Block
{
    // public Block(Dictionary<string, string> patientData, List<Nplicate> nplicates, Color textColour, double qc)
    // {
    //     PatientData = patientData;
    //     Nplicates = nplicates;
    //     TextColour = textColour;
    //     QC = qc;
    // }
    public Block(string[] patientData)
    {
        PatientData = patientData;
    }
    public Block() { }

    public List<Nplicate> Nplicates { get; set; } = new List<Nplicate>();
    // public Dictionary<string, string> PatientData { get; set; } = new Dictionary<string, string>();

    public string[] PatientData { get; set; } = new string[21];
    public Color TextColour { get; set; } = new Color();
    public double QC { get; private set; } = 0;

    public void CalculateQC(Nplicate pos, Nplicate neg)
    {
        QC = pos.Mean == 0 ? double.NaN : (pos.Mean - neg.Mean) / pos.Mean;
    }
}