namespace src.Data;

class Spot
{
    public Spot(double intensity, bool flagged)
    {
        Intensity = intensity;
        IsFlagged = flagged;
    }

    private double _intensity;
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