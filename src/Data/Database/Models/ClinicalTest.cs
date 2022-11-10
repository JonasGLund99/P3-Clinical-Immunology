using Microsoft.AspNetCore.Authentication.OAuth.Claims;
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
        Experiments.Add(experiment);
    }

    public List<Experiment> Experiments = new List<Experiment>();
    public List<string> AnalyteNames = new List<string>();
    public string Title { get; set; }
    public int NplicateSize { get; set; }
    public string Description { get; set; }
    public double MaxRI { get; private set; }
    public double MinRI { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime EditedAt { get; private set; }

    public void AddSlide(Slide slide, Dictionary<string, string> patientData)
    {
        int numOfBlocks = 21;
        slides.Add(slide);
        for (int i = 0; i < numOfBlocks; i++)
        {
            slide.AddBlock(new Block(Guid.NewGuid().ToString(), patientData));
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
            //Read all lines in a file and add each line as an element in a string array
            string[] allLines = File.ReadAllLines(slideDataFiles[i].Path);

            //Find the line in which the information about spots begin
            beginningIndex = Array.FindIndex(allLines, line => start.Match(line).Success);

            //Create string array with only spot information
            string[] spotLines = new string[allLines.Length - beginningIndex];
            allLines.CopyTo(spotLines, beginningIndex + 1);

            //Create array where each entry is a title for spot information
            string[] titles = allLines[beginningIndex].Split("\t");

            List<string> spotInfo = new List<string>();
            nplicatesInBlock = spotLines.Length / NplicateSize;

           
            for (int j = 0; j < slides[i].Blocks.Count; j++)
            {
                for (int k = 0; k < nplicatesInBlock; k++)
                {
                    //Split the line with spotinformation, add the information elements to spotinfo.
                    spotInfo.AddRange(spotLines[k * NplicateSize + (j * nplicatesInBlock * NplicateSize)].Split("\t"));

                    //Find the index in spotInfo that contains the analyteType (ID) and create an Nplicate with it.
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
                        //Calculate the mean and set if the Nplicate are flagged.
                        nplicate.CalculateMean();
                        nplicate.SetFlag();

                        slides[i].Blocks[j].AddNplicate(nplicate);
                }
                Nplicate? pos = slides[i].Blocks[j].Nplicates.Find(nplicate => nplicate.AnalyteType == "pos");
                Nplicate? neg = slides[i].Blocks[j].Nplicates.Find(element => element.AnalyteType == "neg");

                //Calculate the Quality control if the positive and negative control exist
                if (pos == null || neg == null)
                {
                    throw new NullReferenceException("Either the positive or negative control is missing");
                }
                slides[i].Blocks[j].CalculateQC(pos, neg);
            }

            //Calculate the RI for each Nplicate in each block and update max / min RI
            foreach (Block block in slides[i].Blocks)
            {
                Nplicate? neg = block.Nplicates.Find(element => element.AnalyteType == "neg");
                
                if (neg == null)
                {
                    throw new NullReferenceException("The negative control is missing");
                }

                for (int j = 0; j < block.Nplicates.Count; j++)
                {
                    updateMaxMinRI(block.Nplicates[j].CalculateRI(slides[i].Blocks[slides[i].Blocks.Count - 1].Nplicates[j], neg));
                }
            }
        }

        //Set the heatmapcolour of all Nplicates (The RI of all Nplicates in all slides must be calculated before)
        foreach (Slide slide in slides)
        {
            foreach (Block block in slide.Blocks)
            {
                foreach (Nplicate nplicate in block.Nplicates)
                {
                    nplicate.SetHeatMapColour(MaxRI, MinRI);
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

    private void updateMaxMinRI(double RI)
    {
        if (RI > MaxRI)
        {
            MaxRI = RI;
        }
        else if (RI < MinRI)
        {
            MinRI = RI;
        }
    }
    
    public void UpdateEditedAt(DateTime editedAt)
    {
        EditedAt = editedAt;
    }

    public void Delete()
    {
        this.RemoveFromDatabase();

        foreach (Slide slide in slides)
        {
            slide.Delete();
        }
    }

}

class SlideDataFile : BaseModel
{
    public string Path;
    public string Barcode;
}