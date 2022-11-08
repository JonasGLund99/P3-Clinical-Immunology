namespace src.Data;

class Slide : BaseModel
{
    private List<Block> Blocks = new List<Block>();
    public Slide(string barcode, string id) : base(id)
    {
        Barcode = barcode;
    }
    public string Barcode { get; private set; }

    public void AddBlock (Block block)
    {
        Blocks.Add(block);
    }
}
