namespace src.Data;

public class Spot
{
    private double _intensity;
    public Spot(double intensity, bool flagged)
    {
        Intensity = intensity;
        IsFlagged = flagged;
    }
    public double Intensity { 
        get
        {
            return _intensity; 
        }
        set
        {
            if(value < 0)
            {
                throw new ArgumentException("Intensity must be non-negative");
            }
            _intensity = value;
        }
    }

    public bool IsFlagged { get; set; }
}