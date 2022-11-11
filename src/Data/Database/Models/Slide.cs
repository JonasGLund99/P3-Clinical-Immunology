namespace src.Data;

public class Slide : BaseModel<Slide>
{
    public Slide(string barcode, string id) : base(id)
    {
        Barcode = barcode;
        Blocks = new List<Block>();
    }
    
    public string Barcode { get; set; }
    public List<Block> Blocks;
}
