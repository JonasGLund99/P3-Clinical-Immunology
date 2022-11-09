using System.Drawing;

namespace src.Data;

class Nplicate
{
    public Nplicate(string analyteType)
    {
        AnalyteType = analyteType;
    }

    private List<Spot> Spots = new List<Spot>();

    public double RI { get; private set; }
    public double XYZ { get; private set; }
    public double Mean { get; private set; }
    public string AnalyteType { get; }
    public bool IsFlagged { get; private set; }
    public Color HeatmapColour { get; private set; }

    public void AddSpot(Spot spot)
    {
        //Spots.Add(spot);
        //if (Spots.Count == NplicateSize)
        //{
        //    IsFlagged = SetFlag();
        //    Mean = CalculateMean();
        //}
        throw new NotImplementedException("NPLICATESIZE DOES NOT EXIST. Therefore AddSpot is not implemented");
    }

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

    private double CalculateMean()
    {
        double mean = 0;
        foreach (Spot spot in Spots)
        {
            mean += spot.Intensity;
        }
        return mean / Spots.Count;
    }

    public void CalculateRI(Nplicate blank, Nplicate neg)
    {
        XYZ = (Mean - blank.Mean) / neg.Mean;
        RI = Math.Log2(XYZ);
    }

    public void SetHeatMapColour(double max, double min)
    {
        //Start with DarkBlue. Inject Yellow until both Yellow and Blue is max. Then remove Blue. 
        Color Yellow = Color.Yellow;
        Color Blue = Color.DarkBlue;

        //int ROffset = Math.Max(Orange.R, Blue.R);
        //int GOffset = Math.Max(Orange.G, Blue.G);
        //int BOffset = Math.Max(Orange.B, Blue.B);

        //int DeltaR = Math.Abs(Orange.R - Blue.R);
        //int DeltaG = Math.Abs(Orange.G - Blue.G);
        //int DeltaB = Math.Abs(Orange.B - Blue.B);

        //double val = (RI - min) / (max - min);
        //int R = ROffset - Convert.ToByte(DeltaR * (1 - val));
        //int G = GOffset - Convert.ToByte(DeltaG * (1 - val));
        //int B = BOffset - Convert.ToByte(DeltaB * (1 - val));
        //HeatmapColour = Color.FromArgb(255, R, G, B);
    }
}
