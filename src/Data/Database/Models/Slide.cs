namespace src.Data;

public class Slide
{
    public Slide(string barcode)
    {
        Barcode = barcode;
    }
    public Slide() { }
    public Block[] Blocks { get; set; } = new Block[21].Select(b => new Block()).ToArray();
    public string Barcode { get; set; } = "";
}
