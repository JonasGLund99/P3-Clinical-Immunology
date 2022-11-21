namespace src.Data;

public class Slide : BaseModel<Slide>
{
    public Slide(string id) : base(id) { }

    public Slide(string id, string barcode) : base(id)
    {
        Barcode = barcode;
    }
    public Slide(): base() { }
    public Block[] Blocks { get; set; } = new Block[21].Select(b => new Block()).ToArray();
    public string Barcode { get; set; } = "";
}
