using System.Drawing;
namespace src.Data;

public class Block
{
    public Block(List<string> patientData, BlockType type)
    {
        PatientData = patientData;
        Type = type;
    }
    public Block() { }
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