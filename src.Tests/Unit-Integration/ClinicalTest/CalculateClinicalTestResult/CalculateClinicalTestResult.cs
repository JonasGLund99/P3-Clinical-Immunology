using src.Data;
using Xunit;
using OfficeOpenXml;
using System.Collections;

namespace src.Tests;

public class CalculateClinicalTestResultTest
{
    [Theory]
    [ClassData(typeof(CompareExcelAndMockedDataData))]
    public void CompareExcelAndMockedData(double actual, double expected)
    {
        Assert.Equal(expected, actual);
    }

    public class CompareExcelAndMockedDataData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            // Data from excel file
            FileInfo fileInfo = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "../../../Unit-Integration", "ClinicalTest", "CalculateClinicalTestResult", "expectedResults.xlsx"));

            ExcelPackage package = new ExcelPackage(fileInfo);
            ExcelWorkbook workBook = package.Workbook;

            ExcelWorksheet XYZ = workBook.Worksheets["XYZ"];
            ExcelWorksheet Log2 = workBook.Worksheets["Log2"];
            ExcelWorksheet QC = workBook.Worksheets["QualityControl"];

            // Data from clinical test
            ClinicalTest clinicalTest = mockClinicalTest().GetAwaiter().GetResult();
            List<Block> sortedBlocks = clinicalTest.GetSortedBlocks().GetAwaiter().GetResult();
            
            // Compare
            for (int i = 0; i < sortedBlocks.Count; i++)
            {
                for (int j = 0; j < sortedBlocks[i].Nplicates.Count; j++)
                {
                    if (clinicalTest.AnalyteNames[j] == "Empty") continue;
                    yield return new object[] { Math.Round(sortedBlocks[i].Nplicates[j].XYZ, 5), Math.Round((double)XYZ.Cells[i + 1, j + 1].Value, 5) };
                    yield return new object[] { Math.Round(sortedBlocks[i].Nplicates[j].RI, 9), Math.Round((double)Log2.Cells[i + 1, j + 1].Value, 9) };
                }
                yield return new object[] { Math.Round(sortedBlocks[i].QC, 9), Math.Round((double)QC.Cells[i + 1, 1].Value, 9) };
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static async Task<ClinicalTest> mockClinicalTest()
    {
        // Setup database
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();

        // Create clinical test
        ClinicalTest clinicalTest = new ClinicalTest();

        // Add slide data files
        clinicalTest.SlideDataFiles = new List<SlideDataFile>
        {
            new SlideDataFile("10000465_0016_flag.txt", File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "../../../Unit-Integration", "ClinicalTest", "CalculateClinicalTestResult" , "10000465_0016_flag.txt"))),
            new SlideDataFile("10000466_0014_flag.txt", File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "../../../Unit-Integration", "ClinicalTest", "CalculateClinicalTestResult" , "10000466_0014_flag.txt"))),
            new SlideDataFile("10000467_0005_flag.txt", File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "../../../Unit-Integration", "ClinicalTest", "CalculateClinicalTestResult" , "10000467_0005_flag.txt"))),
        };

        // Add Slides
        clinicalTest.Slides = new List<Slide>
        {
            new Slide { Barcode = "10000465" },
            new Slide { Barcode = "10000466" },
            new Slide { Barcode = "10000467" },
        };

        // Add matches between slides and slide data files
        clinicalTest.Matches["10000465_0016_flag.txt"] = 0;
        clinicalTest.Matches["10000466_0014_flag.txt"] = 1;
        clinicalTest.Matches["10000467_0005_flag.txt"] = 2;

        // Initialize empty lists for blocks;
        await clinicalTest.GetNormalBlocks();
        await clinicalTest.GetBlankBlocks();

        // Add blocks
        List<Block> normalBlocks = new List<Block>();
        List<Block> blankBlocks = new List<Block>();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 21; j++)
            {
                if (j < 18)
                {
                    await clinicalTest.AddNormalBlock(new Block("", new List<string>(), Block.BlockType.Normal, i, j, ""));
                }
                else
                {
                    await clinicalTest.AddBlankBlock(new Block("", new List<string>(), Block.BlockType.Blank, i, j, ""));
                }
            }
        }
        
        // Generate overview
        var overview = await clinicalTest.GenerateOverview();

        // Calculate
        await clinicalTest.CalculateClinicalTestResult();

        return clinicalTest;
    }
}