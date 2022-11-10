namespace src.Data;

class Slide
{
    public Slide(string barcode, List<Block> blocks)
    {
        Barcode = barcode;
        Blocks = blocks;
    }
    public Slide(string barcode)
    {
        Barcode = barcode;
    }
    public List<Block> Blocks = new List<Block>();
    public string Barcode { get; private set; }
}
