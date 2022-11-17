using System.Text.RegularExpressions;

namespace src.Data;

public class ClinicalTest : BaseModel<ClinicalTest>
{
    private int nplicatesInBlock { get; set; }
    private int numOfBlocks { get; } = 21;

    public ClinicalTest(string id, string title, int nplicateSize, string description, DateTime createdAt) : base(id)
    {
        Title = title;
        NplicateSize = nplicateSize;
        Description = description;
        CreatedAt = createdAt;
    }
    public ClinicalTest(string id) : base(id) { }
    public ClinicalTest() : base() { }

    public List<SlideDataFile> SlideDataFiles { get; set; } = new List<SlideDataFile>();
    public List<string> TableTitles { get; set; } = new List<string>();
    public string[] ChosenTableTitles { get; set; } = new string[3];
    public List<string> ExperimentIds { get; set; } = new List<string>();
    public List<Slide> Slides { get; set; } = new List<Slide>();
    public List<string> AnalyteNames { get; set; } = new List<string>();
    public string Title { get; set; } = "";
    public int NplicateSize { get; set; } = 3;
    public string Description { get; set; } = "";
    public double MaxRI { get; private set; } = 0;
    public double MinRI { get; private set; } = 0;
    public DateTime CreatedAt { get; } = DateTime.Now;
    public DateTime EditedAt { get; private set; } = DateTime.Now;

    public void AddSlide(Slide slide, string[][] patientData)
    {
        Slides.Add(slide);
        for (int i = 0; i < numOfBlocks; i++)
        {
            slide.Blocks.Add(new Block(patientData[i]));

            // if (i < patientData.Count) {
            //     slide.Blocks.Add(new Block(patientData[i]));
            // } else {
            //     slide.Blocks.Add(new Block(new List<string>()));
            // }
        }
    }

    //public void CreatePatientKeys(List<string> allKeys, params string[] shownKeys)
    //{
    //    PatientKeys.Clear();
    //    ActiveKeys.Clear();

    //    foreach (string key in allKeys)
    //    {
    //        PatientKeys[key] = false;
    //    }

    //    foreach (string key in shownKeys)
    //    {
    //        PatientKeys[key] = true;
    //        ActiveKeys.Add(key);
    //    }
    //}

    //private void UpdatePatientKeys(params string[] newKeys)
    //{
    //    foreach (string key in ActiveKeys)
    //    {
    //        PatientKeys[key] = false;
    //    }
    //    ActiveKeys.Clear();

    //    foreach (string key in newKeys)
    //    {
    //        PatientKeys[key] = true;
    //        ActiveKeys.Add(key);
    //    }        
    //}

    public void ExportClinicalTest(string exportType)
    {
        
    }

    public void CalculateClinicalTestResult()
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
            string[] spotLines = new ArraySegment<string>(allLines, beginningIndex + 1, allLines.Length - beginningIndex - 2).ToArray();
            
            //Create array where each entry is a title for spot information
            string[] titles = allLines[beginningIndex].Split("\t");

            List<string> spotInfo = new List<string>();
            nplicatesInBlock = spotLines.Length / numOfBlocks / NplicateSize;

            for (int j = 0; j < Slides[i].Blocks.Count; j++)
            {
                for (int k = 0; k < nplicatesInBlock; k++)
                {
                    //Split the line with spotinformation, add the information elements to spotinfo.
                    spotInfo.AddRange(spotLines[k * NplicateSize + (j * nplicatesInBlock * NplicateSize)].Split("\t"));

                    //Find the index in spotInfo that contains the analyteType (ID) and create an Nplicate with it.
                    Nplicate nplicate = new Nplicate(spotInfo[Array.IndexOf(titles, "ID")].ToLower());

                    // Add analyteNames when looping through the first block
                    if (j == 0) {
                        AnalyteNames.Add(findSingleSpotInfo(spotInfo, titles, "Name"));
                    }

                    for (int l = 0; l < NplicateSize; l++)
                    {
                        if (l != 0)
                        {
                            spotInfo.AddRange(spotLines[l + (k * NplicateSize) + (j * nplicatesInBlock * NplicateSize)].Split("\t"));
                        }

                        nplicate.Spots.Add(
                            new Spot(
                                intensity: double.Parse(findSingleSpotInfo(spotInfo, titles, "Intensity")), 
                                flagged: findSingleSpotInfo(spotInfo, titles, "Flags") != "0"
                            )
                        );
                        spotInfo.Clear();
                    }
                    //Calculate the mean and set if the Nplicate are flagged.
                    nplicate.CalculateMean();
                    nplicate.SetFlag();

                    Slides[i].Blocks[j].Nplicates.Add(nplicate);
                }
                Nplicate? pos = Slides[i].Blocks[j].Nplicates.Find(nplicate => nplicate.AnalyteType == "pos");
                Nplicate? neg = Slides[i].Blocks[j].Nplicates.Find(nplicate => nplicate.AnalyteType == "neg");

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

    private string findSingleSpotInfo(List<string> spotInfo, string[] titles, string key) 
    {
        return spotInfo[Array.IndexOf(titles, Array.Find(titles, element => element.Contains(key)))].Trim();
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
}


