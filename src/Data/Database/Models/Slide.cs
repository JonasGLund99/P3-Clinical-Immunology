namespace src.Data;

class Slide : BaseModel
{
    public Slide(string id, string barcode) : base(id)
    {
        Barcode = barcode;
    }
    public List<Block> Blocks = new List<Block>();
    public string Barcode { get; private set; }

    public void AddBlock (Block block)
    {
        Blocks.Add(block);
    }
}
