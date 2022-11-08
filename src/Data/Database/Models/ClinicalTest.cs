using System.Globalization;

namespace src.Data;

class ClinicalTest : BaseModel
{
    private List<SlideDataFile> slideDataFiles = new List<SlideDataFile>();
    private List<Slide> slides = new List<Slide>();
    private Dictionary<string, bool> patientKeys = new Dictionary<string, bool>();
    private List<string> activeKeys = new List<string>();
    private int nplicatesInBlock { get; set; }

    public ClinicalTest(string title, int nplicateSize, string description, Experiment experiment, string id) : base(id)
    {
        Title = title;
        NplicateSize = nplicateSize;
        Description = description;
        CreatedAt = DateTime.Now;
        EditedAt = CreatedAt;
        experiments.Add(experiment);
    }

    public List<Experiment> experiments = new List<Experiment>();
    public List<string> AnalyteNames = new List<string>();
    public string Title { get; set; }
    public int NplicateSize { get; set; }
    public string Description { get; set; }
    public double MaxRI { get; private set; }
    public double MinRI { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime EditedAt { get; private set; }

    //private void CalculateNplicatesInBlock(int nplicateSize)
    //{

    //}

    public void AddSlide(Slide slide)
    {
        slides.Add(slide);
    }

   
    public void CreatePatientKeys(List<string> allKeys, params string[] shownKeys)
    {
        patientKeys.Clear();
        activeKeys.Clear();

        foreach (string key in allKeys)
        {
            patientKeys.Add(key, false);
        }

        foreach (string key in shownKeys)
        {
            patientKeys[key] = true;
            activeKeys.Add(key);
        }
    }

    private void UpdatePatientKeys(params string[] newKeys)
    {
        foreach (string key in activeKeys)
        {
            patientKeys[key] = false;
        }
        activeKeys.Clear();

        foreach (string key in newKeys)
        {
            patientKeys[key] = true;
            activeKeys.Add(key);
        }        
    }

    public void AddSlideDataFiles(params SlideDataFile[] slideDataFile)
    {        
        slideDataFiles.AddRange(slideDataFiles);
    }

    public void ExportClinicalTest(string exportType)
    {
        
    }

    private void calculateClinicalTestResult()
    {
        
    }

    private void updateMaxMinRI(double min, double max)
    {
        MaxRI = max;
        MinRI = min;
    }
}

class SlideDataFile : BaseModel
{
    string Path;
    string Barcode;
}