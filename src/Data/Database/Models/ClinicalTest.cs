﻿using System.Text.RegularExpressions;

namespace src.Data;

public class ClinicalTest : BaseModel<ClinicalTest>
{
    private int nplicatesInBlock { get; set; }
    private int numOfBlocks { get; } = 21;
    private List<Block> normalBlocks { get; set; } = new List<Block>();

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
    public List<string> ExperimentIds { get; set; } = new List<string>();
    public List<string> NormalBlockIds { get; set; } = new List<string>();
    public List<Slide> Slides { get; set; } = new List<Slide>();
    public List<string> AnalyteNames { get; set; } = new List<string>();
    public string Title { get; set; } = "";
    public int NplicateSize { get; set; } = 3;
    public string Description { get; set; } = "";
    public double MaxRI { get; set; } = 0;
    public double MinRI { get; set; } = 0;
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public DateTime EditedAt { get; set; } = DateTime.Now;

    public async Task<List<Block>> GetNormalBlocks()
    {
        if (normalBlocks.Count == 0)
        {
            List<Block> result = new List<Block>();

            foreach (string id in NormalBlockIds)
            {
                Block b = await DatabaseService.Instance.GetItemById<Block>(id);
                result.Add(b);
            }
            normalBlocks = result;
        }
        return normalBlocks;
    }
    public void SetNormalBlocks(List<Block> blocks) 
    {
        normalBlocks = blocks;
    }

    public async Task SaveToDatabase(bool saveBlocks)
    {
        if (saveBlocks)
        {
            foreach (Block block in normalBlocks) 
            {
                await block.SaveToDatabase();
            }
        }

        await base.SaveToDatabase();
    }

    public async Task<List<List<Block[]>>> GenerateOverview() 
    {
        int numBlankBlocks = Slides.Select(s => s.BlankBlocks.Count).Sum();
        int totalBlocks = (await GetNormalBlocks()).Count + numBlankBlocks;
        int numPlates = (totalBlocks - 1) / 84 + 1;
        int totalSlides = (totalBlocks - 1) / 21 + 1;
        int totalIndex = 0;

        if (totalBlocks == 0) return new List<List<Block[]>>();

        List<List<Block[]>> overview = new List<List<Block[]>>();

        Slides.Clear();

        for (int i = 0; i < numPlates; i++)
        {
            overview.Add(new List<Block[]>());

            int numSlides = (i * 4) + 4 <= totalSlides ? 4 : totalSlides % 4;

            for (int j = 0; j < numSlides; j++) 
            {
                Slides.Add(new Slide());
                overview[i].Add(new Block[21]);
            }
            for (int j = 0; j < 7; j++)
            {
                for (int k = 0; k < numSlides; k++) 
                {
                    for (int l = 0; l < 3; l++)
                    {
                        int slideIndex = k;
                        int blockIndex = j * 3 + l;
                        if (totalIndex >= totalBlocks)
                        {
                            Block emptyBlock = new Block("Will not be saved", new List<string>(), Block.BlockType.Empty, i, slideIndex);
                            overview[i][slideIndex][blockIndex] = emptyBlock;
                        }
                        else if (Slides[i * 4 + k].BlankBlocks.Exists(b => b.SlideIndex == slideIndex && b.BlockIndex == blockIndex))
                        {
                            Block blankBlock = new Block("Will not be saved", new List<string>(), Block.BlockType.Blank, i, slideIndex);
                            overview[i][slideIndex][blockIndex] = blankBlock;
                        }
                        else
                        {
                            Block normalBlock = new Block(Guid.NewGuid().ToString(), new List<string>(), Block.BlockType.Normal, i, slideIndex);
                            NormalBlockIds.Append(normalBlock.id);
                            overview[i][slideIndex][blockIndex] = normalBlock;
                        }
                        totalIndex++;
                    }
                }
            }
        }
        await SaveToDatabase();
        return overview;
    }
    
    //public void AddSlide(Slide slide, List<string>[] patientData)
    //{
    //    Slides.Add(slide);
    //    for (int i = 0; i < numOfBlocks; i++)
    //    {
    //        if (patientData[i] != null)
    //        {
    //            slide.Blocks[i] = new Block(patientData[i], Block.BlockType.Normal);
    //        }
    //        else
    //        {
    //            slide.Blocks[i] = new Block(new List<string>() { "" }, Block.BlockType.Empty);
    //        }
    //    }
    //}

    public void ExportClinicalTest(string exportType)
    {

    }

    //public void InsertBlankBlock(int slideIndex, int blockIndex)
    //{
    //    if (Slides.Count == 0) return;

    //    List<Block> allBlocks = new List<Block>();

    //    int numPlates = (Slides.Count - 1) / 4 + 1;
    //    int numNonEmptyBlocks = 0;

    //    for (int i = 0; i < numPlates; i++)
    //    {
    //        int numSlides = (i * 4) + 4 <= Slides.Count ? 4 : Slides.Count % 4;
    //        for (int j = 0; j < 7; j++)
    //        {
    //            for (int k = 0; k < numSlides; k++)
    //            {
    //                for (int l = 0; l < 3; l++)
    //                {
    //                    int si = i * 4 + k;
    //                    int bi = j * 3 + l;
    //                    if (si == slideIndex && bi == blockIndex)
    //                    {
    //                        Block blankBlock = new Block();
    //                        blankBlock.Type = Block.BlockType.Blank;
    //                        allBlocks.Add(blankBlock);
    //                    }

    //                    allBlocks.Add(Slides[si].Blocks[bi]);

    //                    if (Slides[si].Blocks[bi].Type != Block.BlockType.Empty)
    //                    {
    //                        numNonEmptyBlocks++;
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    int numPlatesAfterInsert = (numNonEmptyBlocks - 1) / 84 + 1;
    //    int totalSlidesAfterInsert = (numNonEmptyBlocks - 1) / 21 + 1;

    //    if (Slides[Slides.Count - 1].Blocks[20].Type == Block.BlockType.Normal)
    //    {
    //        AddSlide(
    //            slide: new Slide(""),
    //            patientData: new List<string>[21]
    //        );
    //    }
    //    System.Console.WriteLine(allBlocks.Count);

    //    int currentIndex = 0;
    //    for (int i = 0; i < numPlatesAfterInsert; i++)
    //    {
    //        int numSlides = (i * 4) + 4 <= totalSlidesAfterInsert ? 4 : totalSlidesAfterInsert % 4;
    //        System.Console.WriteLine(totalSlidesAfterInsert);
    //        for (int j = 0; j < 7; j++)
    //        {
    //            for (int k = 0; k < numSlides; k++)
    //            {
    //                for (int l = 0; l < 3; l++)
    //                {
    //                    if (currentIndex == allBlocks.Count) continue;

    //                    int si = i * 4 + k;
    //                    int bi = j * 3 + l;
    //                    // System.Console.WriteLine($"{currentIndex}\ti: {i}\tj: {j}\tk: {k}\tl: {l}");
    //                    Slides[si].Blocks[bi] = allBlocks[currentIndex];
    //                    currentIndex++;
    //                }
    //            }
    //        }
    //    }
    //}

    //public void CalculateClinicalTestResult()
    //{
    //    int beginningIndex = 0;
    //    Regex start = new Regex(@"^Block\s*Row\s*Column\s*Name\s*ID", RegexOptions.IgnoreCase);
    //    foreach (SlideDataFile slideDataFile in SlideDataFiles)
    //    {
    //        //Read all lines in a file and add each line as an element in a string array
    //        string[] allLines = slideDataFile.Content.Split("\n");

    //        //Find the line in which the information about spots begin
    //        beginningIndex = Array.FindIndex(allLines, line => start.Match(line).Success);

    //        //Create string array with only spot information
    //        string[] spotLines = new ArraySegment<string>(allLines, beginningIndex + 1, allLines.Length - beginningIndex - 2).ToArray();

    //        //Create array where each entry is a title for spot information
    //        string[] titles = allLines[beginningIndex].Split("\t");

    //        List<string> spotInfo = new List<string>();
    //        nplicatesInBlock = spotLines.Length / numOfBlocks / NplicateSize;

    //        for (int j = 0; j < Slides[Matches[slideDataFile.Filename]].Blocks.Length; j++)
    //        {
    //            for (int k = 0; k < nplicatesInBlock; k++)
    //            {
    //                //Split the line with spotinformation, add the information elements to spotinfo.
    //                spotInfo.AddRange(spotLines[k * NplicateSize + (j * nplicatesInBlock * NplicateSize)].Split("\t"));

    //                //Find the index in spotInfo that contains the analyteType (ID) and create an Nplicate with it.
    //                Nplicate nplicate = new Nplicate(spotInfo[Array.IndexOf(titles, "ID")].ToLower());

    //                // Add analyteNames when looping through the first block
    //                if (j == 0)
    //                {
    //                    AnalyteNames.Add(findSingleSpotInfo(spotInfo, titles, "Name"));
    //                }

    //                for (int l = 0; l < NplicateSize; l++)
    //                {
    //                    if (l != 0)
    //                    {
    //                        spotInfo.AddRange(spotLines[l + (k * NplicateSize) + (j * nplicatesInBlock * NplicateSize)].Split("\t"));
    //                    }
    //                    nplicate.Spots.Add(
    //                        new Spot(
    //                            intensity: double.Parse(findSingleSpotInfo(spotInfo, titles, "Intensity")),
    //                            flagged: findSingleSpotInfo(spotInfo, titles, "Flags") != "0"
    //                        )
    //                    );
    //                    spotInfo.Clear();
    //                }
    //                //Calculate the mean and set if the Nplicate are flagged.
    //                nplicate.CalculateMean();
    //                nplicate.SetFlag();

    //                Slides[Matches[slideDataFile.Filename]].Blocks[j].Nplicates.Add(nplicate);
    //            }
    //            Nplicate? pos = Slides[Matches[slideDataFile.Filename]].Blocks[j].Nplicates.Find(nplicate => nplicate.AnalyteType == "pos");
    //            Nplicate? neg = Slides[Matches[slideDataFile.Filename]].Blocks[j].Nplicates.Find(nplicate => nplicate.AnalyteType == "neg");

    //            //Calculate the Quality control if the positive and negative control exist
    //            if (pos == null || neg == null)
    //            {
    //                throw new NullReferenceException("Either the positive or negative control is missing");
    //            }
    //            Slides[Matches[slideDataFile.Filename]].Blocks[j].CalculateQC(pos, neg);
    //        }

    //        //Calculate the RI for each Nplicate in each block and update max / min RI
    //        foreach (Block block in Slides[Matches[slideDataFile.Filename]].Blocks)
    //        {
    //            Nplicate? neg = block.Nplicates.Find(element => element.AnalyteType == "neg");

    //            if (neg == null)
    //            {
    //                throw new NullReferenceException("The negative control is missing");
    //            }

    //            for (int j = 0; j < block.Nplicates.Count; j++)
    //            {
    //                updateMaxMinRI(block.Nplicates[j].CalculateRI(Slides[Matches[slideDataFile.Filename]].Blocks[Slides[Matches[slideDataFile.Filename]].Blocks.Length - 1].Nplicates[j], neg));
    //            }
    //        }
    //    }
    //    //Set the heatmapcolour of all Nplicates (The RI of all Nplicates in all slides must be calculated before)
    //    foreach (Slide slide in Slides)
    //    {
    //        foreach (Block block in slide.Blocks)
    //        {
    //            foreach (Nplicate nplicate in block.Nplicates)
    //            {
    //                nplicate.SetHeatMapColour(MaxRI, MinRI);
    //            }
    //        }
    //    }
    //}

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


