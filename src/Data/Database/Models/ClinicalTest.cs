﻿namespace src.Data;

public class ClinicalTest : BaseModel<ClinicalTest>
{
    public ClinicalTest(string title, int nplicateSize, string description, Experiment experiment, string id) : base(id)
    {
        Title = title;
        NplicateSize = nplicateSize;
        Description = description;
        PatientKeys = new Dictionary<string, string>();
        Slides = new List<Slide>();
        SlideDataFiles = new List<SlideDataFile>();
        CreatedAt = DateTime.Now;
        EditedAt = CreatedAt;
        Experiments = new List<Experiment>();
        Experiments.Add(experiment);
    }

    public string Title { get; set; }
    public int NplicateSize { get; set; }
    public string Description { get; set; }
    public List<string> AnalyteNames { get; private set; }
    public double MaxRI { get; private set; }
    public double MinRI { get; private set; }
    public Dictionary<string, string> PatientKeys { get; private set; }
    private int NplicatesInBlock { get; set; }
    public List<Slide> Slides { get; private set; }
    public List<SlideDataFile> SlideDataFiles { get; private set; }
    public List<Experiment> Experiments { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime EditedAt { get; private set; }

    //private void CalculateNplicatesInBlock(int nplicateSize)
    //{

    //}
    public void AddSlide(Slide slide)
    {
        
    }

    private void UpdatePatientKeys()
    {
        
    }

    public void AddSlideDataFile(SlideDataFile slideDataFile)
    {
        
        this.SlideDataFiles.Add(slideDataFile);
    }

    public void ExportClinicalTest()
    {
        
    }

    private void CalculateClinicalTestResult()
    {
        
    }

    private void UpdateMaxMinRI(double min, double max)
    {
        MaxRI = max;
        MinRI = min;    
    }
}

public class SlideDataFile : BaseModel<SlideDataFile>
{
    public SlideDataFile(string path, string barcode, string id) : base(id)
    {
        Path = path;
        Barcode = barcode;
    }
    string Path;
    string Barcode;
}
