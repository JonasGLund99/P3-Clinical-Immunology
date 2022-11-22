namespace src.Data;

public class Slide
{
    public Slide() { }

    // public string[] BlockIds { get; set; } = new string[21];
    public List<BlankBlock> BlankBlocks { get; set; } = new List<BlankBlock>();
    // public Block[] Blocks { get; set; } = new Block[21].Select(b => new Block()).ToArray();
    public string Barcode { get; set; } = "";


    
}

