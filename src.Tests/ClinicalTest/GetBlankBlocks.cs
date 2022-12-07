using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using src.Data;
using System.Collections;
using Xunit;

namespace src.Tests;

//https://youtu.be/2Wp8en1I9oQ?t=1152 Se delen om Reusing Instances
public class GetBlankBlocksTest
{
    private readonly ClinicalTest _sut;

    public GetBlankBlocksTest()
    {
        _sut = new ClinicalTest();
    }


    [Theory]
    [ClassData(typeof(GetBlankBlocksTestData))]
    public async void GetBlankBlocks(List<Block> expected, List<Block> mockedBlocks, params ClinicalTest[] clinicalTests)
    {
        List<Block> blocks = new();

        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for GetBlankBlocks test");

        foreach (ClinicalTest c in clinicalTests)
        {
            foreach(Block block in mockedBlocks)
            {
                await c.AddBlankBlock(block);
            }
            blocks.Clear();
            blocks.AddRange(await c.GetBlankBlocks());
        }

        var serializedExpected = JsonConvert.SerializeObject(expected);
        var serializedActual = JsonConvert.SerializeObject(blocks);

        Assert.Equal(serializedExpected, serializedActual);
    }

    [Fact]
    public async void GetBlankBlocksNullException()
    {
        ClinicalTest c2 = new ClinicalTest();
        c2.BlankBlockIds = new List<string>() {
                "Vi ses til julefrokost"
            };

        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for this test");

        await Assert.ThrowsAnyAsync<NullReferenceException>(c2.GetBlankBlocks);

    }

    [Fact]
    public async void GetBlankBlocksEmptyReturnsEmptyList()
    {
        ClinicalTest c2 = new ClinicalTest();

        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for this test");

        List<Block> expected = new List<Block>();
        List<Block> actualBlocks = await c2.GetBlankBlocks();
        var serializedExpected = JsonConvert.SerializeObject(expected);
        var serializedActual = JsonConvert.SerializeObject(actualBlocks);

        Assert.Equal(serializedExpected, serializedActual);

    }

    public class GetBlankBlocksTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            ClinicalTest c1 = new ClinicalTest();
            MockedNormalBlocks mb = new MockedNormalBlocks();


            yield return new object[] { new List<Block>
            {
                new Block { SlideIndex = 0, BlockIndex = 0, Type = Block.BlockType.Blank },
                new Block { SlideIndex = 0, BlockIndex = 1, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 2, Type = Block.BlockType.Blank },
                new Block { SlideIndex = 0, BlockIndex = 3, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 4, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 5, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 6, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 7, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 8, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 9, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 10, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 11, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 12, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 13, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 14, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 15, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 16, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 17, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 18, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 19, Type = Block.BlockType.Blank},
                new Block { SlideIndex = 0, BlockIndex = 20, Type = Block.BlockType.Blank},
            }, mb.Blocks, c1 };

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class MockedNormalBlocks
        {
            private int numSlides = 1;
            public List<Block> Blocks { get; set; }

            public MockedNormalBlocks()
            {
                Blocks = generateBlocks();
            }

            public List<Block> generateBlocks()
            {
                Blocks = new List<Block>();

                for (int j = 0; j < numSlides; j++)
                {
                    for (int i = 0; i < 21; i++)
                    {
                        Block block = new Block();
                        block.SlideIndex = j;
                        block.BlockIndex = i;
                        block.Type = Block.BlockType.Blank;
                        Blocks.Add(block);
                    }
                }


                return Blocks;
            }
        }
    }

}
