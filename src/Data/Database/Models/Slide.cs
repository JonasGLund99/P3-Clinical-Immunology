namespace src.Data;

class Slide : BaseModel
{
    public Slide(string barcode, string id) : base(id)
    {
        Barcode = barcode;
        Blocks = new List<Block>();
    }
    public async Task SaveToDatabase()
    {
        await GenericSaveToDatabase<Slide>();
    }

    public async Task RemoveFromDatabase()
    {
        await GenericRemoveFromDatabase<Slide>();
    }
    public string Barcode { get; set; }
    public List<Block> Blocks;
}
