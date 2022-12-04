using Microsoft.Azure.Cosmos;
using src.Data;
using System.Collections;
using Xunit;
using static System.Reflection.Metadata.BlobBuilder;

namespace src.Tests;

//https://youtu.be/2Wp8en1I9oQ?t=1152 Se delen om Reusing Instances
public class GetSortedBlocks
{
    private readonly ClinicalTest _sut;

    public GetSortedBlocks()
    {
        _sut = new ClinicalTest();
    }


    [Theory]
    [ClassData(typeof(GetNormalBlocksTestData))]
    public async void GetSortedBlocksTheory(List<Block> expected, params ClinicalTest[] clinicalTests)
    {
        List<Block> blocks = new();

        blocks.Sort(delegate (Block x, Block y)
        {
            if (x.SlideIndex == y.SlideIndex)
            {
                return x.BlockIndex - y.BlockIndex;
            }
            return x.SlideIndex - y.SlideIndex;
        });

        Assert.Equal(expected, blocks);
    }

    public class GetNormalBlocksTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            ClinicalTest c1 = new ClinicalTest();
            MockedNormalBlocks mb = new MockedNormalBlocks();
            

            ClinicalTest c2 = new ClinicalTest();

            ClinicalTest c3 = new ClinicalTest();

            yield return new object[] { new List<Block>
            {
                new Block { SlideIndex = 0, BlockIndex = 0 },
                new Block { SlideIndex = 0, BlockIndex = 1 },
                new Block { SlideIndex = 0, BlockIndex = 2 },
                new Block { SlideIndex = 0, BlockIndex = 3 },
                new Block { SlideIndex = 0, BlockIndex = 4 },
                new Block { SlideIndex = 0, BlockIndex = 5 },
                new Block { SlideIndex = 0, BlockIndex = 6 },
                new Block { SlideIndex = 0, BlockIndex = 7 },
                new Block { SlideIndex = 0, BlockIndex = 8 },
                new Block { SlideIndex = 0, BlockIndex = 9 },
                new Block { SlideIndex = 0, BlockIndex = 10 },
                new Block { SlideIndex = 0, BlockIndex = 11 },
                new Block { SlideIndex = 0, BlockIndex = 12 },
                new Block { SlideIndex = 0, BlockIndex = 13 },
                new Block { SlideIndex = 0, BlockIndex = 14 },
                new Block { SlideIndex = 0, BlockIndex = 15 },
                new Block { SlideIndex = 0, BlockIndex = 16 },
                new Block { SlideIndex = 0, BlockIndex = 17 },
                new Block { SlideIndex = 0, BlockIndex = 18 },
                new Block { SlideIndex = 0, BlockIndex = 19 },
                new Block { SlideIndex = 0, BlockIndex = 20 },




            }, c1 };

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class MockedNormalBlocks
        {

            public List<Block> blocks { get; set; }

            public MockedNormalBlocks()
            {
                blocks = generateBlocks();
            }

            public List<Block> generateBlocks()
            {
                blocks = new List<Block>();

                for (int j = 0; j < 2; j++)
                {
                    for (int i = 0; i < 21; i++)
                    {
                        Block block = new Block();
                        block.SlideIndex = j;
                        block.BlockIndex = i;
                        block.Type = Block.BlockType.Normal;
                        blocks.Add(block);
                    }
                }


                return blocks;
            }
        }
    }

}
