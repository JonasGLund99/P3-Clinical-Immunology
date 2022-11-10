using System.IO;
using System.Text.RegularExpressions;

namespace src.Data;

class ClinicalTest : BaseModel<ClinicalTest>
{

    public ClinicalTest(string id, string title, int nplicateSize, string description, DateTime createdAt, DateTime editedAt, List<SlideDataFile> slideDataFiles, Dictionary<string, bool> patientKeys, List<string> activeKeys, int nplicatesInBlock, List<Experiment> experiments, List<Slide> slides, List<string> analyteNames) : base(id)
    {
        Title = title;
        NplicateSize = nplicateSize;
        Description = description;
        CreatedAt = createdAt;
        EditedAt = editedAt;
        SlideDataFiles = slideDataFiles;
        PatientKeys = patientKeys;
        ActiveKeys = activeKeys;
        NplicatesInBlock = nplicatesInBlock;
        Experiments = experiments;
        Slides = slides;
        AnalyteNames = analyteNames;
    }

    public List<SlideDataFile> SlideDataFiles { get; set; } = new List<SlideDataFile>();
    public Dictionary<string, bool> PatientKeys { get; set; } = new Dictionary<string, bool>();
    public List<string> ActiveKeys { get; set; } = new List<string>();
    public int NplicatesInBlock { get; private set; }
    public List<Experiment> Experiments { get; set; } = new List<Experiment>();
    public List<Slide> Slides = new List<Slide>();
    public List<string> AnalyteNames { get; set; } = new List<string>();
    public string Title { get; set; }
    public int NplicateSize { get; set; }
    public string Description { get; set; }
    public double MaxRI { get; private set; }
    public double MinRI { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime EditedAt { get; private set; }

    public void AddSlide(Slide slide, List<Dictionary<string, string>> patientData)
    {
        int numOfBlocks = 21;
        Slides.Add(slide);
        for (int i = 0; i < numOfBlocks; i++)
        {
            slide.Blocks.Add(new Block(patientData[i]));
        }
    }

    public void CreatePatientKeys(List<string> allKeys, params string[] shownKeys)
    {
        PatientKeys.Clear();
        ActiveKeys.Clear();

        foreach (string key in allKeys)
        {
            PatientKeys.Add(key, false);
        }

        foreach (string key in shownKeys)
        {
            PatientKeys[key] = true;
            ActiveKeys.Add(key);
        }
    }

    private void UpdatePatientKeys(params string[] newKeys)
    {
        foreach (string key in ActiveKeys)
        {
            PatientKeys[key] = false;
        }
        ActiveKeys.Clear();

        foreach (string key in newKeys)
        {
            PatientKeys[key] = true;
            ActiveKeys.Add(key);
        }        
    }

    public void AddSlideDataFiles(params SlideDataFile[] slideDataFile)
    {        
        SlideDataFiles.AddRange(slideDataFile);
    }

    public void ExportClinicalTest(string exportType)
    {
        
    }

    private void calculateClinicalTestResult()
    {
        int beginningIndex = 0;
        Regex start = new Regex(@"^Block\s*Row\s*Column\s*Name\s*ID", RegexOptions.IgnoreCase);
        for (int i = 0; i < SlideDataFiles.Count; i++) 
        {
            //Read all lines in a file and add each line as an element in a string array
            string[] allLines = SlideDataFiles[i].Content.Split("\n");

            //Find the line in which the information about spots begin
            beginningIndex = Array.FindIndex(allLines, line => start.Match(line).Success);

            //Create string array with only spot information
            string[] spotLines = new string[allLines.Length - 1 - beginningIndex];
            allLines.CopyTo(spotLines, beginningIndex + 1);
            
            //Create array where each entry is a title for spot information
            string[] titles = allLines[beginningIndex].Split("\t");

            List<string> spotInfo = new List<string>();
            NplicatesInBlock = spotLines.Length / NplicateSize;

           
            for (int j = 0; j < Slides[i].Blocks.Count; j++)
            {
                for (int k = 0; k < NplicatesInBlock; k++)
                {
                    //Split the line with spotinformation, add the information elements to spotinfo.
                    spotInfo.AddRange(spotLines[k * NplicateSize + (j * NplicatesInBlock * NplicateSize)].Split("\t"));

                    //Find the index in spotInfo that contains the analyteType (ID) and create an Nplicate with it.
                    Nplicate nplicate = new Nplicate(spotInfo[Array.IndexOf(titles, "Id")]);

                    for (int l = 0; l < NplicateSize; l++)
                    {                        
                        if (l != 0)
                        {
                            spotInfo.AddRange(spotLines[l + (k * NplicateSize) + (j * NplicatesInBlock * NplicateSize)].Split("\t"));
                        }

                        nplicate.Spots.Add(new Spot(findIntensity(spotInfo, titles), determineIfFlagged(spotInfo, titles)));
                        spotInfo.Clear();
                    }
                        //Calculate the mean and set if the Nplicate are flagged.
                        nplicate.CalculateMean();
                        nplicate.SetFlag();

                        Slides[i].Blocks[j].Nplicates.Add(nplicate);
                }
                Nplicate? pos = Slides[i].Blocks[j].Nplicates.Find(nplicate => nplicate.AnalyteType == "pos");
                Nplicate? neg = Slides[i].Blocks[j].Nplicates.Find(element => element.AnalyteType == "neg");

                //Calculate the Quality control if the positive and negative control exist
                if (pos == null || neg == null)
                {
                    throw new NullReferenceException("Either the positive or negative control is missing");
                }
                Slides[i].Blocks[j].CalculateQC(pos, neg);
            }

            //Calculate the RI for each Nplicate in each block and update max / min RI
            foreach (Block block in Slides[i].Blocks)
            {
                Nplicate? neg = block.Nplicates.Find(element => element.AnalyteType == "neg");
                
                if (neg == null)
                {
                    throw new NullReferenceException("The negative control is missing");
                }

                for (int j = 0; j < block.Nplicates.Count; j++)
                {
                    updateMaxMinRI(block.Nplicates[j].CalculateRI(Slides[i].Blocks[Slides[i].Blocks.Count - 1].Nplicates[j], neg));
                }
            }
        }

        //Set the heatmapcolour of all Nplicates (The RI of all Nplicates in all slides must be calculated before)
        foreach (Slide slide in Slides)
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

    public async Task Delete()
    {
        await RemoveFromDatabase();
    }
}


