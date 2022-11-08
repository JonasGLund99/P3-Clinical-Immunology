namespace src.Data;

class Slide : BaseModel
{
    public Slide(string barcode)
    {
        Barcode = barcode;
        Blocks = new List<Block>();
    }

    public string Barcode { get; set; };
    public List<Block> Blocks;
}
