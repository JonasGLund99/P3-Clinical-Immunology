using System.Drawing;
using System.Security.Cryptography.X509Certificates;

namespace src.Data;

class Nplicate
{
    private List<Spot> spots = new List<Spot>();
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

    public void AddSpot(Spot spot)
    {
        spots.Add(spot);
    }

    public void SetFlag()
    {
        foreach (Spot spot in spots)
        {
            if (spot.IsFlagged)
            {
                IsFlagged = true;
            }
        }
    }

    public void CalculateMean()
    {
        double summedIntensity = 0;
        int notFlagged = 0;

        foreach (Spot spot in spots)
        {
            if (!spot.IsFlagged)
            {
                summedIntensity += spot.Intensity;
                notFlagged++;
            }
        }
        if(notFlagged == 0)
        {
            throw new DivideByZeroException("All spots in n-plicate are flagged");
        }
        Mean = summedIntensity / notFlagged;
    }

    public void CalculateRI(Nplicate blank, Nplicate neg)
    {
        if(neg.Mean == 0)
        {
            throw new DivideByZeroException("The mean of the negative control is 0");
        }
        XYZ = (Mean - blank.Mean) / neg.Mean;
        RI = Math.Log2(XYZ);
    }

    public void SetHeatMapColour(double max, double min)
    {
        if((max - min) == 0)
        {
            throw new DivideByZeroException();
        }
        HeatmapColour = fixer((RI - min) / (max - min));
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
        else if (weight >= 0.25 && weight < 0.5)
        {
            return lerp(low, medium, (weight - 0.25) * 4);
        }
        else if (weight >= 0.5 && weight < 0.75)
        {
            return lerp(medium, high, (weight - 0.5) * 4);
        }
        else if (weight >= 0.75 && weight <= 1)
        {
            return lerp(high, max, (weight - 0.75) * 4);
        }
        else
        {
            throw new ArgumentException("Weight must be between 0 and 1");
        }
    }
}
