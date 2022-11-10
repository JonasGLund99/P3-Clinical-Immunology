namespace src.Data;

class Slide : BaseModel<Slide>
{
    public Slide(string id, string barcode, List<string> blockIds) : base(id)
    {
        Barcode = barcode;
        BlockIds = blockIds;
    }
    public Slide(string id, string barcode) : base(id)
    {
        Barcode = barcode;
    }
    public List<string> BlockIds = new List<string>();
    public List<Block> Blocks = new List<Block>();
    public string Barcode { get; private set; }

    public void AddBlock (Block block)
    {
        Blocks.Add(block);
    }

    public void Delete()
    {
        RemoveFromDatabase();

        foreach (Block block in Blocks)
        {
            block.RemoveFromDatabase();
        }
    }
}
