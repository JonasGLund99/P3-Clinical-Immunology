namespace src.Data;

public class Slide
{
    public Slide() { }

    // public string[] BlockIds { get; set; } = new string[21];
    public List<int> BlankBlockIndicies { get; set; } = new List<int>();
    // public Block[] Blocks { get; set; } = new Block[21].Select(b => new Block()).ToArray();
    public string Barcode { get; set; } = "";


    
}

