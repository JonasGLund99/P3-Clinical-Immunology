using System.Text.RegularExpressions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.Azure.Cosmos;
using Azure;
using System.IO;
using System.Drawing;
using src.Shared;
using OfficeOpenXml.ConditionalFormatting;
using OfficeOpenXml.Drawing.Style.Fill;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Components.Forms;

namespace src.Data;

public class ClinicalTest : BaseModel<ClinicalTest>
{
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
    public async Task AddNormalBlock(Block block) // ONLY USED FOR TESTING
    {
        (await GetNormalBlocks()).Add(block);
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
            ProcessQueue.Instance.Clear();
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
            Experiment? e = await DatabaseService.Instance.GetItemById<Experiment>(id, id);
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
        List<Block> blankBlocks = await GetBlankBlocks();
        foreach (Block block in normBlocks)
        {
            await block.RemoveFromDatabase();
        }
        foreach (Block block in blankBlocks)
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

                        Block? blankBlock = bBlocks.Find(b => b.SlideIndex == totalSlideIndex && b.BlockIndex == blockIndex);
                        if (blankBlock != null)
                        {
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

    public async Task ImportOverview(IBrowserFile file)
    {
        try
        {
            MemoryStream ms = new MemoryStream();
            // copy data from file to memory stream
            await file.OpenReadStream().CopyToAsync(ms);
            // positions the cursor at the beginning of the memory stream
            ms.Position = 0;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // EPPlus license must be specified; otherwise, it throws an exception
            // create ExcelPackage from memory stream
            ExcelPackage package = new ExcelPackage(ms);

            ExcelWorkbook workBook = package.Workbook;
            ExcelWorksheet overview = workBook.Worksheets.FirstOrDefault();
            await GenerateBlocksInSlide(overview);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private async Task GenerateBlocksInSlide(ExcelWorksheet overview)
    {
        TableTitles = new List<string>()
        {
            "Overskrift 1",
            "Overskrift 2",
            "Overskrift 3"
        };
        ChosenTableTitles = new string[3] 
        { 
            "Overskrift 1",
            "Overskrift 2",
            "Overskrift 3" 
        };

        Slides.Clear();
        //Gets all barcodes
        for (int i = 0; i < 4; i++) //i equals the plate number
        {
            int tempI = i;
            for (int j = 0; j < 4; j++) //j is the barcodes from left to right
            {
                int tempJ = j;
                if (overview.Cells[29 + 30 * tempI, 3 + 3 * tempJ].Value != null && overview.Cells[29 + 30 * tempI, 3 + 3 * tempJ].Value.ToString() != null)
                {
                    if(overview.Cells[29 + 30 * tempI, 3 + 3 * tempJ].Value.ToString().Trim() != "")
                    {
                        Slide slide = new Slide();
                        slide.Barcode = overview.Cells[29 + 30 * tempI, 3 + 3 * tempJ].Value.ToString().Trim();
                        Slides.Add(slide);
                    }
                }
            }                    

        }

        foreach (Block blankBlock in await GetBlankBlocks())
        {
            await blankBlock.RemoveFromDatabase();
        }

        foreach (Block normalBlock in await GetNormalBlocks()) 
        {
            await normalBlock.RemoveFromDatabase();
        }

        blankBlocks = null;
        normalBlocks = null;
        BlankBlockIds.Clear();
        NormalBlockIds.Clear();
        //Gets all individual block infomation
        for (int l = 0; l < 4; l++) //Spans the plates
        {
            int tempL = l;
            for (int i = 0; i < 7; i++) //Spans the rows of a plate
            {
                int tempI = i;
                for (int j = 3; j < 15; j++) //Spans the columns of a plate  
                {
                    int tempJ = j;
                    bool blockContainsNoInfo = true;
                    List<string> blockPatientData = new List<string>();
                    for (int k = 0; k < 3; k++) //Spans the rows of a single block
                    {
                        int tempK = k;
                        //i*3 Is the row offset of all blocks before the accessed block.
                        //Cells[8+i*3+k,j]

                        //Throws System.ObjectDisposedException: 'Worksheet has been disposed Object name: 'ExcelWorksheet'.'
                        //When l = 0, i = 6, j = 4, k = 0;
                        if (overview.Cells[8 + tempI * 3 + tempK, tempJ].Value != null && overview.Cells[8 + tempI * 3 + tempK, tempJ].Value.ToString() != null)
                        {
                            blockContainsNoInfo = overview.Cells[8 + tempI * 3 + tempK, tempJ].Value.ToString().Trim() == "" ?
                                                                                true && blockContainsNoInfo : false;
                            blockPatientData.Add(overview.Cells[8 + tempI * 3 + tempK, tempJ].Value.ToString().Trim());
                        }
                    }

                    bool barcodeIsPresent = overview.Cells[29 + 30 * tempL, 3 + 3 * (tempJ / 3 - 1)].Value != null &&
                         overview.Cells[29 + 30 * tempL, 3 + 3 * (tempJ / 3 - 1)].Value.ToString() != null &&
                         overview.Cells[29 + 30 * tempL, 3 + 3 * (tempJ / 3 - 1)].Value.ToString().Trim() != "";

                    if (blockContainsNoInfo && barcodeIsPresent)
                    {
                        Console.WriteLine($"slideIndex = {tempJ / 3 * (tempL + 1) - 1} blockIndex = {tempI * 3 + tempJ % 3}");
                        Block bBlock = new Block(Guid.NewGuid().ToString(), new List<string>(), Block.BlockType.Blank, tempJ / 3 * (tempL + 1) - 1, tempI * 3 + tempJ % 3, this.id);
                        await AddBlankBlock(bBlock);
                    }
                    else if(!blockContainsNoInfo && barcodeIsPresent)
                    {
                        if (blockPatientData.Find(value => value.ToLower() == "blank") != null)
                        {
                            Console.WriteLine($"slideIndex = {tempJ / 3 * (tempL + 1) - 1} blockIndex = {tempI * 3 + tempJ % 3}");
                            Block bBlock = new Block(Guid.NewGuid().ToString(), new List<string>(), Block.BlockType.Blank, tempJ / 3 * (tempL + 1) - 1, tempI * 3 + tempJ % 3, this.id);
                            await AddBlankBlock(bBlock);
                        }
                        else
                        {
                            Console.WriteLine($"slideIndex = {tempJ / 3 * (tempL + 1) - 1} blockIndex = {tempI * 3 + tempJ % 3}");
                            Block normalBlock = new Block(Guid.NewGuid().ToString(), blockPatientData, Block.BlockType.Normal, tempJ / 3 * (tempL + 1) - 1, tempI * 3 + tempJ % 3, this.id);
                            await AddNormalBlock(normalBlock);
                        }
                    }
                }
            }
        }

        Console.WriteLine(normalBlocks.Count);

    }

    public async Task<FileInfo> ExportOverview()
    {
        FileInfo fileInfo = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports", $"{id}-prøve-opsætning.xlsx"));
        if (fileInfo.Exists)
        {
            fileInfo.Delete();  // ensures we create a new workbook
            fileInfo = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports", $"{id}-prøve-opsætning.xlsx"));
        }
        
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // EPPlus license must be specified; otherwise, it throws an exception
        ExcelPackage package = new ExcelPackage(fileInfo);
        ExcelWorkbook workBook = package.Workbook;
        ExcelWorksheet overview = workBook.Worksheets.Add("Prøve opsætning");

        char[] headings = "ABCDEFGH".ToCharArray();
        int row = 5;
        int col;
        int blockNum;
        int slideNum;
        int plateNum = 0;



        foreach (List<Block[]> plate in await GenerateOverview())
        {
            col = 2;
            overview.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Thick;
            overview.Cells[row, col].Style.Border.Left.Style = ExcelBorderStyle.Thick;
            overview.Cells[row, col].Style.Border.Top.Color.SetColor(Color.Gray);
            overview.Cells[row, col].Style.Border.Left.Color.SetColor(Color.Gray);
            overview.Cells[row, col++].Style.Fill.SetBackground(ColorTranslator.FromHtml("#474747"), ExcelFillStyle.Solid);

            for (int i = 1; i <= 12; i++)
            {
                overview.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Thick;
                overview.Cells[row, col].Style.Border.Top.Color.SetColor(Color.Gray);
                overview.Cells[row, col].Value = i;
                overview.Cells[row, col].Style.Font.Color.SetColor(Color.White);
                overview.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                overview.Cells[row, col++].Style.Fill.SetBackground(ColorTranslator.FromHtml("#474747"), ExcelFillStyle.Solid);
            }

            overview.Cells[row, col - 1].Style.Border.Right.Style = ExcelBorderStyle.Thick;
            overview.Cells[row, col - 1].Style.Border.Right.Color.SetColor(Color.Gray);

            col = 2;

            for (int i = 0; i < 8; i++)
            {
                row++;
                overview.Cells[row, col, row + 2, col].Style.Border.Left.Style = ExcelBorderStyle.Thick;
                overview.Cells[row, col, row + 2, col].Style.Border.Left.Color.SetColor(Color.Gray);
                overview.Cells[row , col].Value = headings[i];
                overview.Cells[row, col].Style.Font.Color.SetColor(Color.White);
                overview.Cells[row, col, row + 2, col].Style.Fill.SetBackground(ColorTranslator.FromHtml("#474747"), ExcelFillStyle.Solid);
                overview.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                overview.Cells[row, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                overview.Cells[row, col, row += 2 , col].Merge = true;
            }

            overview.Cells[row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
            overview.Cells[row, col].Style.Border.Bottom.Color.SetColor(Color.Gray);

            slideNum = 0;

            foreach (Block[] slide in plate)
            {
                blockNum = 1;
                row = 6 + (plateNum * 30);
                col = 3 + (slideNum * 3);
                foreach (Block block in slide)
                {
                    overview.Cells[row, col, row + 2, col].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Gray);

                    if (block.Type == Block.BlockType.Blank)
                    {
                        overview.Cells[row + 1, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        overview.Cells[row + 1, col].Style.Font.Color.SetColor(ColorTranslator.FromHtml(block.TextColour));
                        overview.Cells[row + 1, col].Value = "Blank";
                        row += blockNum % 3 == 0 ? 3 : 0;
                        col = blockNum % 3 == 0 ? col - 2 : col + 1;
                        blockNum++;
                        continue;
                    }
                    else if (block.Type == Block.BlockType.Empty)
                    {
                        row += blockNum % 3 == 0 ? 3 : 0;
                        col = blockNum % 3 == 0 ? col - 2 : col + 1;
                        blockNum++;
                        continue;
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        overview.Cells[row + i, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        overview.Cells[row + i, col].Style.Font.Color.SetColor(ColorTranslator.FromHtml(block.TextColour));
                        int titleIndex = findTitleIndex(i);
                        overview.Cells[row + i, col].Value = titleIndex == -1 || titleIndex >= block.PatientData.Count ? "" : block.PatientData[titleIndex];
                    }
                    overview.Cells[row, col, row + 2, col].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Gray);
                    row += blockNum % 3 == 0 ? 3 : 0;
                    col = blockNum % 3 == 0 ? col - 2 : col + 1;
                    blockNum++;
                }

                double dbl;
                bool conv = double.TryParse(Slides[slideNum + (plateNum * 4)].Barcode, out dbl);
                overview.Cells[row, col].Value = conv ? dbl : Slides[slideNum + (plateNum * 4)].Barcode;
                overview.Cells[row, col].Style.Font.Color.SetColor(Color.White);
                overview.Cells[6 + (plateNum * 30), col, 26 + (plateNum * 30), col + 2].Style.Border.BorderAround(ExcelBorderStyle.Thick, Color.Gray);
                overview.Cells[row, col, row + 2, col + 2].Style.Border.BorderAround(ExcelBorderStyle.Thick, Color.Gray);
                overview.Cells[row, col, row + 2, col + 2].Style.Fill.SetBackground(ColorTranslator.FromHtml("#474747"), ExcelFillStyle.Solid);
                overview.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                overview.Cells[row, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                overview.Cells[row, col, row + 2, col + 2].Merge = true;
                slideNum++;
            }
            for (int i = slideNum; i < 4; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        overview.Cells[6 + (plateNum * 30) + (j * 3), col + k + 3, 8 + (plateNum * 30) + (j * 3), col + k + 3].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Gray);
                    }
                }
                overview.Cells[6 + (plateNum * 30), col + 3, 26 + (plateNum * 30), col + 5].Style.Border.BorderAround(ExcelBorderStyle.Thick, Color.Gray);
                overview.Cells[row, col + 3, row + 2, col + 5].Style.Fill.SetBackground(ColorTranslator.FromHtml("#474747"), ExcelFillStyle.Solid);
                overview.Cells[row, col + 3, row + 2, col + 5].Style.Border.BorderAround(ExcelBorderStyle.Thick, Color.Gray);
                overview.Cells[row, col += 3, row + 2, col + 2].Merge = true;
            }
            plateNum++;
            row = 5 + (plateNum * 30);
        }

        package.Save();
        return fileInfo;
    }

    private int findTitleIndex(int i)
    {
        string? title = TableTitles.Find((title) => title == ChosenTableTitles[i]);
        if (title == null)
            return -1;

        return TableTitles.IndexOf(title);
    }

    public async Task ExportHeatmap(ExcelWorkbook workBook, string withFlags)
    {
        ExcelWorksheet heatmap = workBook.Worksheets.Add("Heatmap");
        int row = 2;
        int col = 1;
        int numOfChosenTableTitles = 0;

        heatmap.Cells[row, col].Style.Font.Size = 18;
        heatmap.Cells[row, col].Style.Font.Bold = true;

        heatmap.Cells[row++, col].Value = "Heatmap";

        foreach (string chosenTableTitle in ChosenTableTitles)
        {
            if (chosenTableTitle != "" && chosenTableTitle != null)
            {
                numOfChosenTableTitles++;
                heatmap.Cells[row, col++].Value = chosenTableTitle;
            }
        }

        foreach (string analyteName in AnalyteNames)
        {
            heatmap.Cells[row, col].Style.TextRotation = 90;
            heatmap.Column(col).Width = 5;
            heatmap.Cells[row, col++].Value = analyteName;
        }

        foreach (Block block in await GetSortedBlocks())
        {
            col = 1;
            row++;

            foreach (string chosenTableTitle in ChosenTableTitles)
            {
                if (chosenTableTitle != "" && chosenTableTitle != null)
                {
                    heatmap.Cells[row, col++].Value = block.PatientData[TableTitles.IndexOf(chosenTableTitle)];
                }
            }

            foreach (Nplicate nplicate in block.Nplicates)
            {
                Color heatMapColour = nplicate.HeatmapColour;
                if (withFlags == "withFlags")
                {
                    if (nplicate.GetFlagCount() == NplicateSize)
                    {
                        heatMapColour = ColorTranslator.FromHtml("#ff0000");
                    }
                    else if (nplicate.IsFlagged)
                    {
                        heatmap.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thick, Color.Red);
                    }
                }
                heatmap.Cells[row, col++].Style.Fill.SetBackground(heatMapColour, ExcelFillStyle.Solid);
            }
        }
        heatmap.Columns.AutoFit(3);
        makeColourScale(heatmap, 5, col += 2);
        heatmap.View.FreezePanes(4, numOfChosenTableTitles + 1);
    }

    private void makeColourScale(ExcelWorksheet worksheet, int row, int col)
    {
        worksheet.Cells[row, col, row + 4, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells[row, col, row + 4, col].Merge = true;
        worksheet.Cells[row, col + 1].Value = Math.Round(MaxRI, 1);
        worksheet.Cells[row, col + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        worksheet.Cells[row, col, row + 4, col].Style.Fill.Gradient.Color1.SetColor(Color.FromArgb(255, 249, 235, 46));
        worksheet.Cells[row, col, row += 5, col].Style.Fill.Gradient.Color2.SetColor(Color.FromArgb(255, 76, 194, 108));

        worksheet.Cells[row, col, row + 4, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells[row, col, row + 4, col].Merge = true;
        worksheet.Cells[row, col + 1].Value = Math.Round((MinRI + (MaxRI - MinRI) * 0.75), 1);
        worksheet.Cells[row, col + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        worksheet.Cells[row, col, row + 4, col].Style.Fill.Gradient.Color1.SetColor(Color.FromArgb(255, 76, 194, 108));
        worksheet.Cells[row, col, row += 5, col].Style.Fill.Gradient.Color2.SetColor(Color.FromArgb(255, 32, 140, 141));

        worksheet.Cells[row, col, row + 4, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells[row, col, row + 4, col].Merge = true;
        worksheet.Cells[row, col + 1].Value = Math.Round((MinRI + (MaxRI - MinRI) * 0.50), 1);
        worksheet.Cells[row, col + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        worksheet.Cells[row, col, row + 4, col].Style.Fill.Gradient.Color1.SetColor(Color.FromArgb(255, 32, 140, 141));
        worksheet.Cells[row, col, row += 5, col].Style.Fill.Gradient.Color2.SetColor(Color.FromArgb(255, 62, 78, 138));

        worksheet.Cells[row, col, row + 4, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells[row, col, row + 4, col].Merge = true;
        worksheet.Cells[row, col + 1].Value = Math.Round((MinRI + (MaxRI - MinRI) * 0.25), 1);
        worksheet.Cells[row, col + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        worksheet.Cells[row, col, row + 4, col].Style.Fill.Gradient.Color1.SetColor(Color.FromArgb(255, 62, 78, 138));
        worksheet.Cells[row, col, row += 4, col].Style.Fill.Gradient.Color2.SetColor(Color.FromArgb(255, 68, 1, 88));

        worksheet.Cells[row, col + 1].Value = Math.Round(MinRI, 1);
        worksheet.Cells[row, col + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
    }

    public async Task ExportResultTable(ExcelWorkbook workBook, string withFlags)
    {
        ExcelWorksheet XYZ = workBook.Worksheets.Add("XYZ");
        ExcelWorksheet Log2 = workBook.Worksheets.Add("Log2");
        int XYZRow = 2;
        int XYZCol = 1;
        int Log2Row = 2;
        int Log2Col = 1;
        int numOfChosenTableTitles = 0;
        XYZ.Row(1).Style.Font.Bold = true;
        Log2.Row(1).Style.Font.Bold = true;
        XYZ.Cells.Style.Numberformat.Format = "0.00";
        Log2.Cells.Style.Numberformat.Format = "0.00";
        XYZ.Cells.Style.Numberformat.Format = "_ * #,##0.00_ ;_ * -#,##0.00_ ;_ * \"-\"??_ ;_ @_ ";
        Log2.Cells.Style.Numberformat.Format = "_ * #,##0.00_ ;_ * -#,##0.00_ ;_ * \"-\"??_ ;_ @_ ";

        XYZ.Cells[XYZRow, XYZCol].Style.Font.Size = 18;
        Log2.Cells[Log2Row, XYZCol].Style.Font.Size = 18;
        XYZ.Cells[XYZRow, XYZCol].Style.Font.Bold = true;
        Log2.Cells[Log2Row, XYZCol].Style.Font.Bold = true;

        XYZ.Cells[XYZRow++, XYZCol].Value = "(X-Y)/Z";
        Log2.Cells[Log2Row++, XYZCol].Value = "Log2";

        XYZ.Cells[XYZRow, XYZCol++].Value = "Slide";
        Log2.Cells[Log2Row, Log2Col++].Value = "Slide";

        foreach (string chosenTableTitle in ChosenTableTitles)
        {
            if (chosenTableTitle != "" && chosenTableTitle != null)
            {
                XYZ.Cells[XYZRow, XYZCol++].Value = chosenTableTitle;
                Log2.Cells[Log2Row, Log2Col++].Value = chosenTableTitle;
                numOfChosenTableTitles++;
            }
        }
        XYZ.Cells[XYZRow, XYZCol++].Value = "Pos raw";
        XYZ.Cells[XYZRow, XYZCol++].Value = "Neg raw";
        Log2.Column(Log2Col).Style.Numberformat.Format = "General";
        Log2.Cells[Log2Row, Log2Col++].Value = "Quality control";

        foreach (string analyteName in AnalyteNames)
        {
            XYZ.Cells[XYZRow, XYZCol++].Value = analyteName;
            Log2.Cells[Log2Row, Log2Col++].Value = analyteName;
        }


        foreach (Block block in await GetSortedBlocks())
        {
            XYZCol = 1;
            Log2Col = 1;
            XYZRow++;
            Log2Row++;

            XYZ.Cells[XYZRow, XYZCol++].Value = block.SlideIndex + 1;
            Log2.Cells[Log2Row, Log2Col++].Value = block.SlideIndex + 1;

            foreach (var chosenTableTitle in ChosenTableTitles)
            {
                if (chosenTableTitle != "" && chosenTableTitle != null)
                {
                    XYZ.Cells[XYZRow, XYZCol++].Value = block.PatientData[TableTitles.IndexOf(chosenTableTitle)];
                    Log2.Cells[Log2Row, Log2Col++].Value = block.PatientData[TableTitles.IndexOf(chosenTableTitle)];
                }
            }
            Nplicate? pos = block.Nplicates.Find(nplicate => nplicate.AnalyteType == "pos");
            Nplicate? neg = block.Nplicates.Find(nplicate => nplicate.AnalyteType == "neg");
            if (pos == null || neg == null)
            {
                XYZ.Cells[XYZRow, XYZCol++].Value = "-";
                XYZ.Cells[XYZRow, XYZCol++].Value = "-";
            }
            else
            {
                XYZ.Cells[XYZRow, XYZCol++].Value = pos.Mean;
                XYZ.Cells[XYZRow, XYZCol++].Value = neg.Mean;
            }
            Log2.Cells[Log2Row, Log2Col++].Value = block.QC;

            foreach (Nplicate nplicate in block.Nplicates)
            {
                if (withFlags == "withFlags")
                {
                    if (nplicate.GetFlagCount() == NplicateSize)
                    {
                        XYZ.Cells[XYZRow, XYZCol].Style.Fill.SetBackground(ColorTranslator.FromHtml("#ff0000"), ExcelFillStyle.Solid);
                        Log2.Cells[Log2Row, Log2Col].Style.Fill.SetBackground(ColorTranslator.FromHtml("#ff0000"), ExcelFillStyle.Solid);
                    }
                    else if (nplicate.IsFlagged)
                    {
                        XYZ.Cells[XYZRow, XYZCol].Style.Border.BorderAround(ExcelBorderStyle.Thick, Color.Red);
                        Log2.Cells[Log2Row, Log2Col].Style.Border.BorderAround(ExcelBorderStyle.Thick, Color.Red);
                    }
                }
                XYZ.Cells[XYZRow, XYZCol++].Value = nplicate.XYZ;
                Log2.Cells[Log2Row, Log2Col++].Value = nplicate.RI;
            }
        }
        XYZ.Columns.AutoFit();
        Log2.Columns.AutoFit();
        XYZ.View.FreezePanes(4, numOfChosenTableTitles + 1);
        Log2.View.FreezePanes(4, numOfChosenTableTitles + 1);
    }

    public async Task ExportPatientData(ExcelWorkbook workBook)
    {
        ExcelWorksheet PatientData = workBook.Worksheets.Add("Patient data");

        int patientDataRow = 2;
        int patientDataCol = 1;
        PatientData.Row(1).Style.Font.Bold = true;
        PatientData.Cells.Style.Numberformat.Format = "0.00";
        PatientData.Cells.Style.Numberformat.Format = "_ * #,##0.00_ ;_ * -#,##0.00_ ;_ * \"-\"??_ ;_ @_ ";

        PatientData.View.FreezePanes(4, 2);

        PatientData.Cells[patientDataRow, patientDataCol].Style.Font.Size = 18;
        PatientData.Cells[patientDataRow, patientDataCol].Style.Font.Bold = true;

        PatientData.Cells[patientDataRow++, patientDataCol].Value = "Patient data";
        PatientData.Cells[patientDataRow, patientDataCol++].Value = "Slide";
        
        foreach (string key in TableTitles)
        {
            PatientData.Cells[patientDataRow, patientDataCol++].Value = key;
        }
        patientDataRow++;
        patientDataCol = 1;
        foreach (Block block in await GetSortedBlocks())
        {
            PatientData.Cells[patientDataRow, patientDataCol++].Value = block.SlideIndex + 1;
            foreach (string data in block.PatientData)
            {
                double dbl;
                bool conv = double.TryParse(data, out dbl);
                PatientData.Cells[patientDataRow, patientDataCol++].Value = conv ? dbl : data;
            }
            patientDataCol = 1;
            patientDataRow++;
        }
    }

    public async Task<FileInfo> ExportResultTableAndHeatmap(string withFlags)
    {
        FileInfo fileInfo = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports", $"{id}-result.xlsx"));
        if (fileInfo.Exists)
        {
            fileInfo.Delete();  // ensures we create a new workbook
            fileInfo = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports", $"{id}-result.xlsx"));
        }

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // EPPlus license must be specified; otherwise, it throws an exception
        ExcelPackage package = new ExcelPackage(fileInfo);
        ExcelWorkbook workBook = package.Workbook;

        await ExportHeatmap(workBook, withFlags);
        await ExportResultTable(workBook, withFlags);
        await ExportPatientData(workBook);
        package.Save();
        return fileInfo;
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
            string[] spotLines = slideDataFile.GetSpotLines();

            //Create array where each entry is a title for spot information
            string[] titles = slideDataFile.GetTitles();

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
                                flagged: Array.Find(titles, t => t.Contains("Flags")) != null && findSingleSpotInfo(spotInfo, titles, "Flags") != "0"
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

            //Calculate the RI for each Nplicate in each normal block and update max / min RI
            foreach (Block block in normBlocksInSlide)
            {
                Nplicate? neg = block.Nplicates.Find(element => element.AnalyteType == "neg");

                if (neg == null)
                {
                    throw new NullReferenceException("The negative control is missing");
                }

                for (int j = 0; j < block.Nplicates.Count; j++)
                {
                    //Block lastBlankBlock = bBlocksInSlide.Last();
                    Block? calculatorBlock = bBlocksInSlide.Find(b => b.IsCalculatorBlock);
                    if (calculatorBlock != null)
                    {
                        updateMaxMinRI(block.Nplicates[j].CalculateRI(calculatorBlock.Nplicates[j], neg));
                    }
                    else
                    {
                        throw new ArgumentNullException();
                    }
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
