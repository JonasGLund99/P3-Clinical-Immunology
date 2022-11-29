using System.Text.RegularExpressions;
using Microsoft.Azure.Cosmos;


namespace src.Data;

public class ClinicalTest : BaseModel<ClinicalTest>
{
    private List<Slide> slides { get; set; } = new List<Slide>();
    private int nplicatesInBlock { get; set; }
    private int numOfBlocks { get; } = 21;
    private List<Block>? normalBlocks { get; set; } = null;
    private List<Block>? blankBlocks { get; set; } = null;

    public ClinicalTest(string id, string title, int nplicateSize, string description, DateTime createdAt) : base(id)
    {
        PartitionKey = id;
        Title = title;
        NplicateSize = nplicateSize;
        Description = description;
        CreatedAt = createdAt;
    }
    public ClinicalTest(string id) : base(id)
    {
        PartitionKey = id;
    }
    public ClinicalTest() : base() { }

    public override string PartitionKey { get; set; } = "";

    //Dictionary conisting of a filename and its matching slide
    public Dictionary<string, int> Matches = new Dictionary<string, int>();
    public List<SlideDataFile> SlideDataFiles { get; set; } = new List<SlideDataFile>();
    public List<string> TableTitles { get; set; } = new List<string>();
    public string[] ChosenTableTitles { get; set; } = new string[3];
    public List<string> ExperimentIds { get; set; } = new List<string>();
    public List<string> NormalBlockIds { get; set; } = new List<string>();
    public List<string> BlankBlockIds { get; set; } = new List<string>();
    public List<Slide> Slides { get; set; } = new List<Slide>();
    public List<string> AnalyteNames { get; set; } = new List<string>();
    public string Title { get; set; } = "";
    public int NplicateSize { get; set; } = 3;
    public string Description { get; set; } = "";
    public double MaxRI { get; set; } = 0;
    public double MinRI { get; set; } = 0;
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public DateTime EditedAt { get; set; } = DateTime.Now;
    public bool IsEmpty { get; set; } = true;
    public bool CalculationNecessary {get; set; } = true;

    // Normal blocks
    public async Task<List<Block>> GetNormalBlocks()
    {
        if (normalBlocks == null)
        {
            List<Block> result = new List<Block>();

            foreach (string id in NormalBlockIds)
            {
                Block? b = await DatabaseService.Instance.GetItemById<Block>(id, this.PartitionKey);
                if (b == null) throw new NullReferenceException("Block is null");
                result.Add(b);
            }
            normalBlocks = result;
        }
        return normalBlocks;
    }
    public async Task<List<Block>> GetSortedBlocks()
    {
        List<Block> blocks = new();
        blocks.AddRange(await GetNormalBlocks());

        blocks.Sort(delegate (Block x, Block y)
        {
            if (x.SlideIndex == y.SlideIndex)
            {
                return x.BlockIndex - y.BlockIndex;
            }
            return x.SlideIndex - y.SlideIndex;
        });
        return blocks;
    }
    public void SetNormalBlocks(List<Block> blocks) 
    {
        normalBlocks = blocks;
    }

    // Blank blocks
    public async Task<List<Block>> GetBlankBlocks()
    {
        if (blankBlocks == null)
        {
            List<Block> result = new List<Block>();

            foreach (string id in BlankBlockIds)
            {
                Block? b = await DatabaseService.Instance.GetItemById<Block>(id, this.PartitionKey);
                if (b == null) throw new NullReferenceException("Block is null");
                result.Add(b);
            }
            blankBlocks = result;
        }
        return blankBlocks;
    }
    public async Task AddBlankBlock(Block block)
    {
        (await GetBlankBlocks()).Add(block);
    }
    public async Task RemoveBlankBlock(Block block)
    {
        (await GetBlankBlocks()).Remove(block);
    }

    public async void SaveToDatabase(bool saveBlocks)
    {
        if (saveBlocks)
        {
            ProcessQueue.Instance.Clear(this.id);
            List<Block> normBlocks = await GetNormalBlocks();
            List<Block> bBlocks = await GetBlankBlocks();
            NormalBlockIds.Clear();
            BlankBlockIds.Clear();
            foreach (Block block in normBlocks)
            {
                block.SaveToDatabase();
                NormalBlockIds.Add(block.id);
            }
            foreach (Block block in bBlocks)
            {
                block.SaveToDatabase();
                BlankBlockIds.Add(block.id);
            }
        }

        base.SaveToDatabase();
    }
    public override async Task SaveToDatabaseAsync()
    {
        foreach (string id in ExperimentIds)
        {
            Experiment? e = await ExperimentManager.GetExperimentById(id);
            if (e == null) continue;
            e.EditedAt = DateTime.Now;
            await e.SaveToDatabaseAsync();
        }
        EditedAt = DateTime.Now;
        await base.SaveToDatabaseAsync();
    }

    public override async Task RemoveFromDatabase()
    {
        List<Block> normBlocks = await GetNormalBlocks();
        foreach (Block block in normBlocks)
        {
            await block.RemoveFromDatabase();
        }
        await base.RemoveFromDatabase();
    }

    public async Task<List<List<Block[]>>> GenerateOverview()
    {
        List<Block> normBlocks = await GetNormalBlocks();
        List<Block> bBlocks = await GetBlankBlocks();

        int numBlankBlocks = bBlocks.Count;
        int totalBlocks = normBlocks.Count + numBlankBlocks;
        int numPlates = (totalBlocks - 1) / 84 + 1;
        int totalSlides = (totalBlocks - 1) / 21 + 1;
        int normalBlockIndex = 0;

        if (totalBlocks == 0) return new List<List<Block[]>>();

        List<List<Block[]>> overview = new List<List<Block[]>>();

        if (totalSlides > Slides.Count)
        {
            while (totalSlides > Slides.Count)
            {
                Slides.Add(new Slide());
            }
        }
        else if (Slides.Count > totalSlides)
        {
            while (Slides.Count > totalSlides)
            {
                Slides.RemoveAt(Slides.Count - 1);
            }
        }


        for (int i = 0; i < numPlates; i++)
        {
            int plateIndex = i;

            overview.Add(new List<Block[]>());

            int numSlides = (i * 4) + 4 <= totalSlides ? 4 : totalSlides % 4;

            for (int j = 0; j < numSlides; j++)
            {
                overview[i].Add(new Block[21]);
            }
            for (int j = 0; j < 7; j++)
            {
                for (int k = 0; k < numSlides; k++)
                {
                    for (int l = 0; l < 3; l++)
                    {
                        int slideIndex = k;
                        int totalSlideIndex = plateIndex * 4 + slideIndex;
                        int blockIndex = j * 3 + l;

                        if (bBlocks.Exists(b => b.SlideIndex == totalSlideIndex && b.BlockIndex == blockIndex))
                        {
                            Block blankBlock = bBlocks.Find(b => b.SlideIndex == totalSlideIndex && b.BlockIndex == blockIndex);
                            overview[plateIndex][slideIndex][blockIndex] = blankBlock;
                        }
                        else if (normalBlockIndex < normBlocks.Count)
                        {
                            Block normalBlock = normBlocks[normalBlockIndex];
                            normalBlock.SlideIndex = totalSlideIndex;
                            normalBlock.BlockIndex = blockIndex;
                            overview[plateIndex][slideIndex][blockIndex] = normalBlock;
                            normalBlockIndex++;
                        }
                        else
                        {
                            Block emptyBlock = new Block("Will not be saved", new List<string>(), Block.BlockType.Empty, totalSlideIndex, blockIndex, this.id);
                            overview[i][slideIndex][blockIndex] = emptyBlock;
                        }
                    }
                }
            }
        }
        SaveToDatabase(true);
        return overview;
    }

    public void ExportClinicalTest(string exportType)
    {

    }

    public async Task CalculateClinicalTestResult()
    {
        Regex start = new Regex(@"^Block\s*Row\s*Column", RegexOptions.IgnoreCase);

        List<Block> normBlocks = await GetNormalBlocks();
        List<Block> bBlocks = await GetBlankBlocks();

        AnalyteNames.Clear();

        foreach (SlideDataFile slideDataFile in SlideDataFiles)
        {
            //Create string array with only spot information
            string[] spotLines = slideDataFile.getSpotLines();

            //Create array where each entry is a title for spot information
            string[] titles = slideDataFile.getTitles();

            List<string> spotInfo = new List<string>();
            nplicatesInBlock = spotLines.Length / numOfBlocks / NplicateSize;

            Slide slide = Slides[Matches[slideDataFile.Filename]];

            List<Block> normBlocksInSlide = normBlocks.Where(b => b.SlideIndex == Slides.IndexOf(slide)).ToList();
            List<Block> bBlocksInSlide = bBlocks.Where(b => b.SlideIndex == Slides.IndexOf(slide)).ToList();

            List<Block> allBlocksInSlide = normBlocksInSlide.Concat(bBlocksInSlide).ToList();
            allBlocksInSlide.Sort((b1, b2) => b1.BlockIndex - b2.BlockIndex);

            for (int j = 0; j < allBlocksInSlide.Count; j++)
            {
                allBlocksInSlide[j].Nplicates.Clear();
                for (int k = 0; k < nplicatesInBlock; k++)
                {
                    //Split the line with spotinformation, add the information elements to spotinfo.
                    spotInfo.AddRange(spotLines[k * NplicateSize + (j * nplicatesInBlock * NplicateSize)].Split("\t"));

                    //Find the index in spotInfo that contains the analyteType (ID) and create an Nplicate with it.
                    Nplicate nplicate = new Nplicate(spotInfo[Array.IndexOf(titles, "ID")].ToLower());

                    // Add analyteNames when looping through the first block
                    if (j == 0 && SlideDataFiles.IndexOf(slideDataFile) == 0)
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

                    allBlocksInSlide[j].Nplicates.Add(nplicate);
                }
                Nplicate? pos = allBlocksInSlide[j].Nplicates.Find(nplicate => nplicate.AnalyteType == "pos");
                Nplicate? neg = allBlocksInSlide[j].Nplicates.Find(nplicate => nplicate.AnalyteType == "neg");

                //Calculate the Quality control if the positive and negative control exist
                if (pos == null || neg == null)
                {
                    throw new NullReferenceException("Either the positive or negative control is missing");
                }
                allBlocksInSlide[j].CalculateQC(pos, neg);
            }

            //Calculate the RI for each Nplicate in each block and update max / min RI
            foreach (Block block in allBlocksInSlide)
            {
                Nplicate? neg = block.Nplicates.Find(element => element.AnalyteType == "neg");

                if (neg == null)
                {
                    throw new NullReferenceException("The negative control is missing");
                }

                for (int j = 0; j < block.Nplicates.Count; j++)
                {
                    Block lastBlankBlock = bBlocksInSlide.Last();
                    updateMaxMinRI(block.Nplicates[j].CalculateRI(lastBlankBlock.Nplicates[j], neg));
                }
            }
        }

        //Set the heatmapcolour of all Nplicates (The RI of all Nplicates in all slides must be calculated before)
        foreach (Block block in normBlocks)
        {
            foreach (Nplicate nplicate in block.Nplicates)
            {
                nplicate.SetHeatMapColour(MaxRI, MinRI);
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


