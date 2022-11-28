using System.Text.RegularExpressions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.Azure.Cosmos;
using Azure;
using System.IO;
using System.Drawing;

namespace src.Data;

public class ClinicalTest : BaseModel<ClinicalTest>
{
    private List<Slide> slides { get; set; } = new List<Slide>();
    private int nplicatesInBlock { get; set; }
    private int numOfBlocks { get; } = 21;
    private List<Block>? normalBlocks { get; set; } = null;
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

    public async Task SaveToDatabase(bool saveBlocks)
    {
        if (saveBlocks)
        {
            List<Block> normBlocks = await GetNormalBlocks();
            NormalBlockIds.Clear();
            foreach (Block block in normBlocks) 
            {
                await block.SaveToDatabase();
                NormalBlockIds.Add(block.id);
            }
        }

        await base.SaveToDatabase();
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
        int numBlankBlocks = Slides.Select(s => s.BlankBlockIndicies.Count).Sum();
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
            while(Slides.Count > totalSlides)
            {
                Slides.RemoveAt(Slides.Count - 1);
            }
        }
        

        for (int i = 0; i < numPlates; i++)
        {
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
                        int blockIndex = j * 3 + l;
                        if (Slides[i * 4 + k].BlankBlockIndicies.Contains(blockIndex))
                        {
                            Block blankBlock = new Block("Will not be saved", new List<string>(), Block.BlockType.Blank, i, slideIndex, this.id);
                            overview[i][slideIndex][blockIndex] = blankBlock;
                        }
                        else if (normalBlockIndex < normBlocks.Count)
                        {
                            Block normalBlock = normBlocks[normalBlockIndex];
                            NormalBlockIds.Append(normalBlock.id);
                            normalBlock.SlideIndex = slideIndex;
                            normalBlock.BlockIndex = blockIndex;
                            overview[i][slideIndex][blockIndex] = normalBlock;
                            normalBlockIndex++; 
                        }
                        else 
                        {
                            Block emptyBlock = new Block("Will not be saved", new List<string>(), Block.BlockType.Empty, i, slideIndex, this.id);
                            overview[i][slideIndex][blockIndex] = emptyBlock;
                        }
                    }
                }
            }
        }
        await SaveToDatabase();
        return overview;
    }

    public async Task<FileInfo> ExportResultTable(string withFlags)
    {
        FileInfo fileInfo = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(),"wwwroot", "exports", $"{id}.xlsx"));
        if (fileInfo.Exists)
        {
            fileInfo.Delete();  // ensures we create a new workbook
            fileInfo = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports", $"{id}.xlsx"));
        }

        ExcelPackage package = new ExcelPackage(fileInfo);
        ExcelWorkbook workBook = package.Workbook;
        ExcelWorksheet XYZ = workBook.Worksheets.Add("XYZ");
        ExcelWorksheet Log2 = workBook.Worksheets.Add("Log2");
        int XYZRow = 2;
        int XYZCol = 1;
        int Log2Row = 2;
        int Log2Col = 1;
        XYZ.Row(1).Style.Font.Bold = true;
        Log2.Row(1).Style.Font.Bold = true;
        XYZ.Cells.Style.Numberformat.Format = "0.00";
        Log2.Cells.Style.Numberformat.Format = "0.00";
        XYZ.Cells.Style.Numberformat.Format = "_ * #,##0.00_ ;_ * -#,##0.00_ ;_ * \"-\"??_ ;_ @_ ";
        Log2.Cells.Style.Numberformat.Format = "_ * #,##0.00_ ;_ * -#,##0.00_ ;_ * \"-\"??_ ;_ @_ ";


        XYZ.View.FreezePanes(4, 5);
        Log2.View.FreezePanes(4, 5);
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
            XYZ.Cells[XYZRow, XYZCol++].Value = chosenTableTitle;
            Log2.Cells[Log2Row, Log2Col++].Value = chosenTableTitle;
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
                XYZ.Cells[XYZRow, XYZCol++].Value = block.PatientData[TableTitles.IndexOf(chosenTableTitle)];
                Log2.Cells[Log2Row, Log2Col++].Value = block.PatientData[TableTitles.IndexOf(chosenTableTitle)];
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
                    if (nplicate.IsFlagged)
                    {
                        setFlagsBorderXYZ(XYZ, XYZRow, XYZCol);
                        setFlagsBorderLog2(Log2, Log2Row, Log2Col);
                    }
                }
                XYZ.Cells[XYZRow, XYZCol++].Value = nplicate.XYZ;
                Log2.Cells[Log2Row, Log2Col++].Value = nplicate.RI;
            }
        }
        XYZ.Columns.AutoFit();
        Log2.Columns.AutoFit();
        await package.SaveAsync();
        return fileInfo;
    }

    private void setFlagsBorderXYZ(ExcelWorksheet XYZ, int row, int col)
    {
        XYZ.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Thick;
        XYZ.Cells[row, col].Style.Border.Left.Style = ExcelBorderStyle.Thick;
        XYZ.Cells[row, col].Style.Border.Right.Style = ExcelBorderStyle.Thick;
        XYZ.Cells[row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;


        XYZ.Cells[row, col].Style.Border.Top.Color.SetColor(Color.Red);
        XYZ.Cells[row, col].Style.Border.Left.Color.SetColor(Color.Red);
        XYZ.Cells[row, col].Style.Border.Right.Color.SetColor(Color.Red);
        XYZ.Cells[row, col].Style.Border.Bottom.Color.SetColor(Color.Red);
    }

    private void setFlagsBorderLog2(ExcelWorksheet Log2, int row, int col)
    {
        Log2.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Thick;
        Log2.Cells[row, col].Style.Border.Left.Style = ExcelBorderStyle.Thick;
        Log2.Cells[row, col].Style.Border.Right.Style = ExcelBorderStyle.Thick;
        Log2.Cells[row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;

        Log2.Cells[row, col].Style.Border.Top.Color.SetColor(Color.Red);
        Log2.Cells[row, col].Style.Border.Left.Color.SetColor(Color.Red);
        Log2.Cells[row, col].Style.Border.Right.Color.SetColor(Color.Red);
        Log2.Cells[row, col].Style.Border.Bottom.Color.SetColor(Color.Red);
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


