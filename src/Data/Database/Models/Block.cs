using System.Drawing;
namespace src.Data;

public class Block : BaseModel<Block>
{
    public Block(string id, List<string> patientData, BlockType type, int plateIndex, int slideIndex) : base(id)
    {
        PatientData = patientData;
        Type = type;
        PlateIndex = plateIndex;
        SlideIndex = slideIndex;
    }
    public Block() : base() { }

    public int PlateIndex { get; set; } = 0;
    public int SlideIndex { get; set; } = 0;
    public BlockType Type { get; set; } = Block.BlockType.Empty;
    public List<Nplicate> Nplicates { get; set; } = new List<Nplicate>();
    public List<string> PatientData { get; set; } = new List<string>();
    public Color TextColour { get; set; } = new Color();
    public double QC { get; set; } = 0;

    public void CalculateQC(Nplicate pos, Nplicate neg)
    {
        QC = pos.Mean == 0 ? double.NaN : (pos.Mean - neg.Mean) / pos.Mean;
    }

    public enum BlockType { Normal, Blank, Empty }
}

public struct BlankBlock
{
    public BlankBlock(int slideIndex, int blockIndex)
    {
        SlideIndex = slideIndex;
        BlockIndex = blockIndex;
    }
    public int SlideIndex { get; set; }
    public int BlockIndex { get; set; }
}