using System.Drawing;

namespace src.Data;

public class Block : BaseModel<Block>
{
    public Block(List<string> patientData, string id) : base(id)
    {
        PatientData = patientData;
    }


    List<string> PatientData;
    Color TextColour = new Color();
    double QualityControl;

    public void AddNplicate(Nplicate nplicate)
    {
        
    }

    public void CalculateQC()
    {
        
    }
}