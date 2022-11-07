namespace src.Data;

class Block : BaseModel
{
    public Block(List<string> patientData, )
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