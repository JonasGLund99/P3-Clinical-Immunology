using System.Drawing;
namespace src.Data;

public class Block : BaseModel<Block>
{
    public Block(string id, List<string> patientData, BlockType type, int plateIndex, int slideIndex, string partitionKey) : base(id)
    {
        PartitionKey = partitionKey;
        PatientData = patientData;
        Type = type;
        PlateIndex = plateIndex;
        SlideIndex = slideIndex;
    }
    public Block() : base() { }

    public override string PartitionKey { get; set; } = "";
    public int PlateIndex { get; set; } = 0;
    public int SlideIndex { get; set; } = 0;
    public BlockType Type { get; set; } = Block.BlockType.Empty;
    public List<Nplicate> Nplicates { get; set; } = new List<Nplicate>();
    public List<string> PatientData { get; set; } = new List<string>();
    public Color TextColour { get; set; } = new Color();
    public double QC { get; set; } = 0;
    public int Index { get; set; } = default;
    public void CalculateQC(Nplicate pos, Nplicate neg)
    {
        QC = pos.Mean == 0 ? double.NaN : (pos.Mean - neg.Mean) / pos.Mean;
    }

    // public override async Task SaveToDatabase() 
    // {
    //     await base.SaveToDatabase();
    // }

    public enum BlockType { Normal, Blank, Empty }
}
