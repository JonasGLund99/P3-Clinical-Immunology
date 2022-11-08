using System.Drawing;

namespace src.Data;

class Block : BaseModel
{
    public Block(List<string> patientData, string id) : base(id)
    {
        PatientData = patientData;
    }


    List<string> PatientData;
    Color TextColour = new Color();
    double QualityControl;

    public async Task SaveToDatabase()
    {
        await GenericSaveToDatabase<Block>();
    }

    public async Task RemoveFromDatabase()
    {
        await GenericRemoveFromDatabase<Block>();
    }

    public void AddNplicate(Nplicate nplicate)
    {
        
    }

    public void CalculateQC()
    {
        
    }
}