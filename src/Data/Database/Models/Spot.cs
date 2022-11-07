namespace src.Data;

class Spot
{
    public Spot(decimal intensity, bool flagged)
    {
        Intensity = intensity;
        Flagged = flagged;
    }

    private decimal _intesity;
    public decimal Intensity { 
        get
        {
            return _intesity; 
        }
        set
        {
            if(value < 0)
            {
                throw new ArgumentException("Intensity must be non-negative");
            }
            _intesity = value;
        }
    }

    public bool IsFlagged { get; set; }
}