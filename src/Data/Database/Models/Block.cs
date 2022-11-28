using System.Drawing;
namespace src.Data;

public class Block : BaseModel<Block>
{
    public Block(string id, List<string> patientData, BlockType type, int slideIndex, int blockIndex, string partitionKey) : base(id)
    {
        PartitionKey = partitionKey;
        PatientData = patientData;
        Type = type;
        SlideIndex = slideIndex;
        BlockIndex = blockIndex;
    }
    public Block() : base() { }

    public override string PartitionKey { get; set; } = "";
    public int SlideIndex { get; set; } = 0;
    public BlockType Type { get; set; } = Block.BlockType.Empty;
    public List<Nplicate> Nplicates { get; set; } = new List<Nplicate>();
    public List<string> PatientData { get; set; } = new List<string>();
    public string TextColour { get; set; } = "#212529";
    public double QC { get; set; } = 0;
    public int BlockIndex { get; set; } = 0;


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
