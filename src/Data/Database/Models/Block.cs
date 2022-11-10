using System.Drawing;

namespace src.Data;

class Block : BaseModel
{
    public Block(string id, Dictionary<string, string> patientData) : base(id)
    {
        PatientData = patientData;
    }

    public List<Nplicate> Nplicates = new List<Nplicate>();
    Dictionary<string, string> PatientData;
    Color TextColour = new Color();
    public double QC { get; private set; }

    public void AddNplicate(Nplicate nplicate)
    {
        Nplicates.Add(nplicate);
    }

    public void CalculateQC(Nplicate pos, Nplicate neg)
    {
        QC = (pos.Mean - neg.Mean) / pos.Mean;
    }
    public void SetTextColour(Color textColour)
    {
        TextColour = textColour;
    }
}