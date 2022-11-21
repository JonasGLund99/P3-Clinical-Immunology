using System.Text.RegularExpressions;
using Microsoft.Azure.Cosmos;


namespace src.Data;

public class ClinicalTest : BaseModel<ClinicalTest>
{
    private List<Slide> slides { get; set; } = new List<Slide>();
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

    //Dictionary conisting of a filename and its matching slide
    public Dictionary<string, int> Matches = new Dictionary<string, int>();
    public List<SlideDataFile> SlideDataFiles { get; set; } = new List<SlideDataFile>();
    public List<string> TableTitles { get; set; } = new List<string>();
    public string[] ChosenTableTitles { get; set; } = new string[3];
    public List<string> SlideIds { get; set; } = new List<string>();
    public List<string> ExperimentIds { get; set; } = new List<string>();
    public List<string> AnalyteNames { get; set; } = new List<string>();
    public string Title { get; set; } = "";
    public int NplicateSize { get; set; } = 3;
    public string Description { get; set; } = "";
    public double MaxRI { get; set; } = 0;
    public double MinRI { get; set; } = 0;
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public DateTime EditedAt { get; set; } = DateTime.Now;

    public async Task<Slide> GetSlideById(string id)
    {
        Slide slide = await DatabaseService.Instance.GetItemById<Slide>(id);
        return slide;
    }

    public void AddSlide(Slide slide, List<string>[] patientData)
    {
        slides.Add(slide);
        SlideIds.Add(slide.id);
        for (int i = 0; i < numOfBlocks; i++)
        {
            if (patientData[i] != null) 
            {
                slide.Blocks[i] = new Block(patientData[i]);
            } 
            else 
            {
                slide.Blocks[i] = new Block(new List<string>() { "" });
            }
        }
    }

    public void ExportClinicalTest(string exportType)
    {
        
    }

    public List<Slide> GetSlides()
    {
        return slides;
    }

    public void SetSlide(Slide slide)
    {
        slides.Add(slide);
    }

    public override async Task SaveToDatabase()
    {
        Database? db = DatabaseService.Instance.Database;

        if (db == null) throw new NullReferenceException("There was no reference to the database");

        foreach (Slide slide in slides)
        {
            await db.GetContainer("Slide").UpsertItemAsync<Slide>(
            item: slide
            );
        }

        await db.GetContainer("ClinicalTest").UpsertItemAsync<ClinicalTest>(
            item: this
        );
    }


    public override async Task RemoveFromDatabase()
    {
        Database? db = DatabaseService.Instance.Database;

        if (db == null) throw new NullReferenceException("There was no reference to the database");
        foreach (Slide slide in slides)
        {
            await db.GetContainer("Slide").DeleteItemAsync<Slide>(
                id: slide.id,
                partitionKey: new PartitionKey(slide.id)
            );
        }

        await db.GetContainer("ClinicalTest").DeleteItemAsync<ClinicalTest>(
            id: this.id,
            partitionKey: new PartitionKey(this.id)
        );
    }

    public void CalculateClinicalTestResult()
    {
        int beginningIndex = 0;
        Regex start = new Regex(@"^Block\s*Row\s*Column\s*Name\s*ID", RegexOptions.IgnoreCase);
        foreach (SlideDataFile slideDataFile in SlideDataFiles) {
            //Read all lines in a file and add each line as an element in a string array
            string[] allLines = slideDataFile.Content.Split("\n");

            //Find the line in which the information about spots begin
            beginningIndex = Array.FindIndex(allLines, line => start.Match(line).Success);

            //Create string array with only spot information
            string[] spotLines = new ArraySegment<string>(allLines, beginningIndex + 1, allLines.Length - beginningIndex - 2).ToArray();

            //Create array where each entry is a title for spot information
            string[] titles = allLines[beginningIndex].Split("\t");

            List<string> spotInfo = new List<string>();
            nplicatesInBlock = spotLines.Length / numOfBlocks / NplicateSize;

            for (int j = 0; j < slides[Matches[slideDataFile.Filename]].Blocks.Length; j++)
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

                    slides[Matches[slideDataFile.Filename]].Blocks[j].Nplicates.Add(nplicate);
                }
                Nplicate? pos = slides[Matches[slideDataFile.Filename]].Blocks[j].Nplicates.Find(nplicate => nplicate.AnalyteType == "pos");
                Nplicate? neg = slides[Matches[slideDataFile.Filename]].Blocks[j].Nplicates.Find(nplicate => nplicate.AnalyteType == "neg");

                //Calculate the Quality control if the positive and negative control exist
                if (pos == null || neg == null)
                {
                    throw new NullReferenceException("Either the positive or negative control is missing");
                }
                slides[Matches[slideDataFile.Filename]].Blocks[j].CalculateQC(pos, neg);
            }

            //Calculate the RI for each Nplicate in each block and update max / min RI
            foreach (Block block in slides[Matches[slideDataFile.Filename]].Blocks)
            {
                Nplicate? neg = block.Nplicates.Find(element => element.AnalyteType == "neg");
                
                if (neg == null)
                {
                    throw new NullReferenceException("The negative control is missing");
                }

                for (int j = 0; j < block.Nplicates.Count; j++)
                {
                    updateMaxMinRI(block.Nplicates[j].CalculateRI(slides[Matches[slideDataFile.Filename]].Blocks[slides[Matches[slideDataFile.Filename]].Blocks.Length - 1].Nplicates[j], neg));
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


