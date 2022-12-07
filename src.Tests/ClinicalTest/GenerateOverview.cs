using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using OfficeOpenXml.Core;
using src.Data;
using System.Collections;
using Xunit;

namespace src.Tests;

//https://youtu.be/2Wp8en1I9oQ?t=1152 Se delen om Reusing Instances
public class GenerateOverviewTest
{
    public GenerateOverviewTest()
    {
    }


    [Theory]
    [ClassData(typeof(GenerateOverviewTestData))]
    public async void GenerateOverview(List<List<Block[]>> expected, params ClinicalTest[] clinicalTests)
    {
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for SaveToDatabase test in ClinicalTest");
        
        List<List<Block[]>>? overview = null; 

        foreach (ClinicalTest clinicalTest in clinicalTests)
        {
            overview = await clinicalTest.GenerateOverview();
        }

        // Act
        Assert.NotNull(overview);

        var serializedExpected = JsonConvert.SerializeObject(expected);
        var serializedActual = JsonConvert.SerializeObject(overview);

        Assert.Equal(serializedExpected, serializedActual);
    }


    public class GenerateOverviewTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            MockedBlankBlocks mBB = new MockedBlankBlocks();
            MockedNormalBlocks mNB = new MockedNormalBlocks();
            MockedMixedBlocks mMB = new MockedMixedBlocks();

            string guidClinicalTest1 = Guid.NewGuid().ToString();
            ClinicalTest c1 = new ClinicalTest { id = guidClinicalTest1, PartitionKey = guidClinicalTest1 };
            SetBlocks(mBB.Blocks, true, c1);

            string guidClinicalTest2 = Guid.NewGuid().ToString();
            ClinicalTest c2 = new ClinicalTest { id = guidClinicalTest2, PartitionKey = guidClinicalTest2 };
            SetBlocks(mNB.Blocks, false, c2);

            string guidClinicalTest3 = Guid.NewGuid().ToString();
            ClinicalTest c3 = new ClinicalTest { id = guidClinicalTest2, PartitionKey = guidClinicalTest2 };

            SetBlocks(mMB.Blocks, false, c3);
            SetBlocks(mMB.BlankBlocks, true, c3);


            yield return new object[] { new List<List<Block[]>>
            {
               new List<Block[]> {
                   new Block[]
                   {
                        new Block {SlideIndex = 0, BlockIndex = 0, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 1, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 2, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 3, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 4, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 5, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 6, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 7, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 8, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 9, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 10, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 11, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 12, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 13, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 14, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 15, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 16, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 17, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 18, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 19, Type = Block.BlockType.Blank },
                        new Block {SlideIndex = 0, BlockIndex = 20, Type = Block.BlockType.Blank },
                   }
               }
            }, c1 };
            
            
            yield return new object[] { new List<List<Block[]>>
            {
               new List<Block[]> {
                   new Block[]
                   {
                        new Block {SlideIndex = 0, BlockIndex = 0, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 1, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 2, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 3, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 4, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 5, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 6, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 7, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 8, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 9, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 10, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 11, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 12, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 13, Type = Block.BlockType.Normal},
                        new Block {SlideIndex = 0, BlockIndex = 14, Type = Block.BlockType.Normal},
                        new Block {SlideIndex = 0, BlockIndex = 15, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 16, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 17, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 18, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 19, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 20, Type = Block.BlockType.Normal },
                   }
               }
            }, c2 };
            
            
            yield return new object[] { new List<List<Block[]>>
            {
               new List<Block[]> {
                   new Block[]
                   {
                        new Block {SlideIndex = 0, BlockIndex = 0, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 1, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 2, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 3, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 4, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 5, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 6, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 7, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 8, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 9, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 10, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 11, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 12, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 13, Type = Block.BlockType.Normal},
                        new Block {SlideIndex = 0, BlockIndex = 14, Type = Block.BlockType.Blank},
                        new Block {SlideIndex = 0, BlockIndex = 15, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 16, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 17, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 18, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 19, Type = Block.BlockType.Normal },
                        new Block {SlideIndex = 0, BlockIndex = 20, Type = Block.BlockType.Normal },
                   }
               }
            }, c3 };
        }

        public async void SetBlocks(List<Block> blocks, bool isBlank, ClinicalTest c)
        {
            foreach(Block block in blocks)
            {
                if(isBlank)
                {
                    await c.AddBlankBlock(block);
                }
                else
                {
                    await c.AddNormalBlock(block);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
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
                    block.Type = Block.BlockType.Normal;
                    Blocks.Add(block);
                }
            }


            return Blocks;
        }
    }

    private class MockedBlankBlocks
    {
        private int numSlides = 1;
        public List<Block> Blocks { get; set; }

        public MockedBlankBlocks()
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

    private class MockedMixedBlocks
    {
        private int numSlides = 1;
        public List<Block> Blocks { get; set; }
        public List<Block> BlankBlocks { get; set; } = new List<Block>();


        public MockedMixedBlocks()
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
                    if(i == 14)
                    {
                        block.Type = Block.BlockType.Blank;
                        BlankBlocks.Add(block);
                    }
                    else
                    {
                        block.Type = Block.BlockType.Normal;
                        Blocks.Add(block);
                    }
                }
            }

            return Blocks;
        }
    }
}



//public class BlockClinicalTestRemoveFromDatabase : Block { }
//public class ClinicalTestRemoveFromDatabase : ClinicalTest { }