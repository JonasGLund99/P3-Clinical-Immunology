namespace src.Data;

public class Slide
{
    public Slide(string barcode)
    {
        Barcode = barcode;
    }
    public Slide(){ }
    public List<Block> Blocks = new List<Block>();
    public string Barcode { get; private set; } = "";
}
