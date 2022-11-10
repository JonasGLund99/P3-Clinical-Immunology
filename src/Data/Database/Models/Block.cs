using System.Drawing;

namespace src.Data;

class Block : BaseModel<Block>
{
    public Block(string id, Dictionary<string, string> patientData, List<Nplicate> nplicates, Color textColour, double qc) : base(id)
    {
        PatientData = patientData;
        Nplicates = nplicates;
        TextColour = textColour;
        QC = qc;
    }
    public Block(string id, Dictionary<string, string> patientData) : base(id)
    {
        PatientData = patientData;
    }

    public List<Nplicate> Nplicates = new List<Nplicate>();
    public Dictionary<string, string> PatientData;
    public Color TextColour = new Color();
    public double QC { get; private set; }

    public void AddNplicate(Nplicate nplicate)
    {
        Nplicates.Add(nplicate);
    }

    public void CalculateQC(Nplicate pos, Nplicate neg)
    {
        QC = pos.Mean == 0 ? double.NaN : (pos.Mean - neg.Mean) / pos.Mean;
    }
}