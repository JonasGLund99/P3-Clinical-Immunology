using System.IO;
using System.Text.RegularExpressions;

namespace src.Data;

class ClinicalTest : BaseModel
{
    private List<SlideDataFile> slideDataFiles = new List<SlideDataFile>();
    private List<Slide> slides = new List<Slide>();
    private Dictionary<string, bool> patientKeys = new Dictionary<string, bool>();
    private List<string> activeKeys = new List<string>();
    private int nplicatesInBlock { get; set; }

    public ClinicalTest(string id, string title, int nplicateSize, string description, Experiment experiment) : base(id)
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

    public void AddSlide(Slide slide, Dictionary<string, string> patientData)
    {
        int numOfBlocks = 21;
        slides.Add(slide);
        for (int i = 0; i < numOfBlocks; i++)
        {
            slide.AddBlock(new Block("id", patientData));
        }
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
        slideDataFiles.AddRange(slideDataFile);
    }

    public void ExportClinicalTest(string exportType)
    {
        
    }

    private void calculateClinicalTestResult()
    {
        int beginningIndex = 0;
        Regex start = new Regex(@"^Block\s*Row\s*Column\s*Name\s*ID", RegexOptions.IgnoreCase);
        for (int i = 0; i < slideDataFiles.Count; i++) 
        {
            string[] allLines = File.ReadAllLines(slideDataFiles[i].Path);
            beginningIndex = Array.FindIndex(allLines, line => start.Match(line).Success);
            string[] spotLines = new string[allLines.Length - beginningIndex];
            allLines.CopyTo(spotLines, beginningIndex + 1);
            string[] titles = allLines[beginningIndex].Split("\t");
            List<string> spotInfo = new List<string>();
            nplicatesInBlock = spotLines.Length / NplicateSize;

            for (int j = 0; j < slides[i].Blocks.Count; j++)
            {
                for (int k = 0; k < nplicatesInBlock; k++)
                {
                    spotInfo.AddRange(spotLines[k * NplicateSize + (j * nplicatesInBlock * NplicateSize)].Split("\t"));
                    Nplicate nplicate = new Nplicate(spotInfo[Array.IndexOf(titles, "Id")]);
                    for (int l = 0; l < NplicateSize; l++)
                    {
                        if (l != 0)
                        {
                            spotInfo.AddRange(spotLines[l + (k * NplicateSize) + (j * nplicatesInBlock * NplicateSize)].Split("\t"));
                        }
                        nplicate.AddSpot(new Spot(findIntensity(spotInfo, titles), determineIfFlagged(spotInfo, titles)));
                        spotInfo.Clear();
                    }
                        nplicate.CalculateMean();
                        nplicate.SetFlag();
                }
                Nplicate? pos = slides[i].Blocks[j].Nplicates.Find(nplicate => nplicate.AnalyteType == "pos");
                Nplicate? neg = slides[i].Blocks[j].Nplicates.Find(element => element.AnalyteType == "neg");

                if (pos == null || neg== null)
                {
                    throw new NullReferenceException("Either the positive or negative control is missing");
                }
                slides[i].Blocks[j].CalculateQC(pos, neg);
            }

            foreach (Block block in slides[i].Blocks)
            {
                Nplicate? neg = block.Nplicates.Find(element => element.AnalyteType == "neg");
                
                if (neg == null)
                {
                    throw new NullReferenceException("The negative control is missing");
                }

                foreach (Nplicate nplicate in block.Nplicates)
                {
                    nplicate.CalculateRI(slides[i].Blocks[slides[i].Blocks.Count - 1].Nplicates[], neg);
                }
            }
        }
    }
    private double findIntensity(List<string> spotInfo, params string[] titles)
    {
        return double.Parse(spotInfo[Array.IndexOf(titles, Array.Find(titles, element => element.Contains("Intensity")))]);
    }
    private bool determineIfFlagged(List<string> spotInfo, params string[] titles)
    {
        return spotInfo[Array.IndexOf(titles, Array.Find(titles, element => element.Contains("Flags")))] != "0";
    }

    private void updateMaxMinRI(double min, double max)
    {
        MaxRI = max;
        MinRI = min;
    }
}

class SlideDataFile : BaseModel
{
    public string Path;
    public string Barcode;
}