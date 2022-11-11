using System.Drawing;

namespace src.Data;

public class Nplicate
{
    public Nplicate(double ri, double xyz, double mean, string analyteType, bool isFlagged, Color heatmapColour, List<Spot> spots)
    {
        RI = ri;
        XYZ = xyz;
        Mean = mean;
        AnalyteType = analyteType;
        IsFlagged = isFlagged;
        HeatmapColour = heatmapColour;
        Spots = spots;
    }
    public Nplicate(string analyteType)
    {
        AnalyteType = analyteType;
    }
    public double RI { get; private set; }
    public double XYZ { get; private set; }
    public double Mean { get; private set; }
    public string AnalyteType { get; }
    public bool IsFlagged { get; private set; }
    public Color HeatmapColour { get; private set; }
    public List<Spot> Spots = new List<Spot>();

    public void SetFlag()
    {
        foreach (Spot spot in Spots)
        {
            if (spot.IsFlagged)
            {
                IsFlagged = true;
                break;
            }
        }
    }

    public void CalculateMean()
    {
        double summedIntensity = 0;
        int numValidSpots = 0;

        foreach (Spot spot in Spots)
        {
            if (!spot.IsFlagged)
            {
                summedIntensity += spot.Intensity;
                numValidSpots++;
            }
        }

        Mean = numValidSpots == 0 ? 0 : summedIntensity / numValidSpots;
    }

    public double CalculateRI(Nplicate correspondingBlank, Nplicate neg)
    {
        XYZ = neg.Mean == 0 ? double.NaN : (Mean - correspondingBlank.Mean) / neg.Mean;
        RI = XYZ < 1 ? 0 : Math.Log2(XYZ);
        return RI;
    }

    public void SetHeatMapColour(double maxRI, double minRI)
    {
        if((maxRI - minRI) == 0)
        {
            throw new DivideByZeroException("The Min and Max are the same");
        }
        HeatmapColour = RI == double.NaN ? Color.Red : fixer((RI - minRI) / (maxRI - minRI));
    }

    private Color lerp(Color colour1, Color colour2, double weight)
    {
        double flippedWeight = (1 - weight);
        double a = colour1.A * flippedWeight + colour2.A * weight;
        double r = colour1.R * flippedWeight + colour2.R * weight;
        double g = colour1.G * flippedWeight + colour2.G * weight;
        double b = colour1.B * flippedWeight + colour2.B * weight;
        return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
    }

    private Color fixer(double weight)
    {
        //Colours from the heatmap scale provided by CIAUH
        Color max = Color.FromArgb(255, 249, 235, 46);
        Color high = Color.FromArgb(255, 76, 194, 108);
        Color medium = Color.FromArgb(255, 32, 140, 141);
        Color low = Color.FromArgb(255, 62, 78, 138);
        Color min = Color.FromArgb(255, 68, 1, 88);

        if (weight >= 0 && weight < 0.25)
        {
            return lerp(min, low, weight * 4);
        }
        else if (weight < 0.5)
        {
            return lerp(low, medium, (weight - 0.25) * 4);
        }
        else if (weight < 0.75)
        {
            return lerp(medium, high, (weight - 0.5) * 4);
        }
        else if (weight <= 1)
        {
            return lerp(high, max, (weight - 0.75) * 4);
        }
        else
        {
            throw new ArgumentException("Weight must be between 0 and 1");
        }
    }
}
