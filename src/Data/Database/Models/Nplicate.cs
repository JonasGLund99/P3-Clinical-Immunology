namespace src.Data;

class Nplicate
{
    public Nplicate(string analyteType)
    {
        AnalyteType = analyteType;
    }

    private List<Spot> Spots = new List<Spot>();
    
    public decimal RI;
    public decimal XYZ;
    public decimal Mean;
    public string AnalyteType;
    public bool IsFlagged { get; set; }
    public double HeatmapColour { get; set; }

    private bool SetFlag()
    {
        foreach (Spot spot in Spots)
        {
            if (spot.IsFlagged)
            {
                return true;
            }
        }
        return false;
    }

    private decimal CalculateMean()
    {
        decimal mean = 0;
        foreach (Spot spot in Spots)
        {
            mean += spot.Intensity;
        }
        return mean / Spots.Count;
    }
    
    public void AddSpot(Spot spot)
    {
        Spots.Add(spot);
        if (Spots.Count == NplicateSize)
        {
            IsFlagged = SetFlag();
            Mean = CalculateMean();
        }
    }
}
