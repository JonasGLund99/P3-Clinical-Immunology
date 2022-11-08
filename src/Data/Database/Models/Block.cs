using System.Drawing;

namespace src.Data;

class Block : BaseModel
{
    private List<Nplicate> nplicates = new List<Nplicate>();
    public Block(Dictionary<string, string> patientData, string id) : base(id)
    {
        PatientData = patientData;
    }


    Dictionary<string, string> PatientData;
    Color TextColour = new Color();
    public double QC { get; private set; }


    public void AddNplicate(Nplicate nplicate)
    {
        nplicates.Add(nplicate);
    }

    public void CalculateQC(Nplicate pos, Nplicate neg)
    {
        QC = (pos.Mean - neg.Mean) / pos.Mean;
    }
    public void SetTextColour()
    {
        throw new NotImplementedException();
    }
    //Consider whether to place this function on Slide or maybe Clinical Test
    public void ReadLine()
    {
        throw new NotImplementedException();
    }
}