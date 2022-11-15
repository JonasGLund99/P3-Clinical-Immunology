using System.Text.RegularExpressions;
using OfficeOpenXml;
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
    public Dictionary<string, bool> PatientKeys { get; set; } = new Dictionary<string, bool>();
    public List<string> ActiveKeys { get; set; } = new List<string>();
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

    public void AddSlide(Slide slide, List<Dictionary<string, string>> patientData)
    {
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

    public void ExportResultTable() 
    {
        FileInfo fileInfo = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports", $"{this.id}.xlsx"));

        if (fileInfo.Exists) fileInfo.Delete();

        ExcelPackage package = new ExcelPackage(fileInfo);
        ExcelWorksheet wsXYZ = package.Workbook.Worksheets.Add("XYZ");
        ExcelWorksheet wsLog2 = package.Workbook.Worksheets.Add("log2");

        wsXYZ.SetValue(1, 1, "Slide");
        wsLog2.SetValue(1, 1, "Slide");
        for (int i = 0; i < AnalyteNames.Count; i++) 
        {
            wsXYZ.SetValue(1, i + 2, AnalyteNames[i]);
            wsLog2.SetValue(1, i + 2, AnalyteNames[i]);
        }
        for (int i = 0; i < Slides.Count; i++) 
        {
            for (int j = 0; j < Slides[i].Blocks.Count; j++) 
            {
                wsXYZ.SetValue((j + 2) + (i * numOfBlocks), j + 1, i + 1);
                wsLog2.SetValue((j + 2) + (i * numOfBlocks), j + 1, i + 1);

                for (int k = 0; k < Slides[i].Blocks[j].Nplicates.Count; k++)
                {
                    wsXYZ.SetValue((j + 2) + (i * numOfBlocks), k + 2, Slides[i].Blocks[j].Nplicates[k].XYZ);

                    wsLog2.SetValue((j + 2) + (i * numOfBlocks), k + 2, Slides[i].Blocks[j].Nplicates[k].RI);
                }
            }
        }

        package.Save();
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

            // Find all slides where the barcode is included in the slideDataFile (Should only be 1)
            List<Slide> matchedSlides = Slides.FindAll(s => SlideDataFiles[i].Filename.Contains(s.Barcode));
            if (matchedSlides.Count != 1) throw new Exception("Didn't match exactly one slide");

            Slide currentSlide = matchedSlides[0];

            for (int j = 0; j < currentSlide.Blocks.Count; j++)
            {
                for (int k = 0; k < nplicatesInBlock; k++)
                {
                    //Split the line with spotinformation, add the information elements to spotinfo.
                    spotInfo.AddRange(spotLines[k * NplicateSize + (j * nplicatesInBlock * NplicateSize)].Split("\t"));

                    //Find the index in spotInfo that contains the analyteType (ID) and create an Nplicate with it.
                    Nplicate nplicate = new Nplicate(spotInfo[Array.IndexOf(titles, "ID")].ToLower());

                    // Add analyteNames when looping through the first block
                    if (i == 0 && j == 0) 
                    {
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

                    currentSlide.Blocks[j].Nplicates.Add(nplicate);
                }
                Nplicate? pos = currentSlide.Blocks[j].Nplicates.Find(nplicate => nplicate.AnalyteType == "pos");
                Nplicate? neg = currentSlide.Blocks[j].Nplicates.Find(nplicate => nplicate.AnalyteType == "neg");

                //Calculate the Quality control if the positive and negative control exist
                if (pos == null || neg == null)
                {
                    throw new NullReferenceException("Either the positive or negative control is missing");
                }
                currentSlide.Blocks[j].CalculateQC(pos, neg);
            }

            //Calculate the RI for each Nplicate in each block and update max / min RI
            foreach (Block block in currentSlide.Blocks)
            {
                Nplicate? neg = block.Nplicates.Find(element => element.AnalyteType == "neg");
                
                if (neg == null)
                {
                    throw new NullReferenceException("The negative control is missing");
                }

                for (int j = 0; j < block.Nplicates.Count; j++)
                {
                    updateMaxMinRI(block.Nplicates[j].CalculateRI(currentSlide.Blocks[currentSlide.Blocks.Count - 1].Nplicates[j], neg));
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


