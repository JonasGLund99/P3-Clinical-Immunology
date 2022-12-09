using src.Data;
using System.Collections;
using Xunit;
namespace src.Tests;

public class CalculateQCTest
{
    private readonly Block _sut;

    public CalculateQCTest()
    {
        _sut = new Block();
    }

    [Theory]
    [ClassData(typeof(CalculateQCTestData))]
    public void CalculateQCTheory(double expected, Nplicate pos, Nplicate neg, params Block[] blocks)
    {
        foreach (Block block in blocks)
        {
            block.CalculateQC(pos, neg);
            _sut.QC = block.QC;
        }

        Assert.Equal(expected, _sut.QC);
    }

    public class CalculateQCTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            Block testBlock1 = new Block();
            Block testBlock2 = new Block();
            Block testBlock3 = new Block();



            Nplicate posBlock1 = new Nplicate("K20");
            posBlock1.Spots.Add(new Spot(24588904, false));
            posBlock1.Spots.Add(new Spot(21498584, false));
            posBlock1.Spots.Add(new Spot(26103096, false));
            posBlock1.Mean = 24063528;

            Nplicate negBlock1 = new Nplicate("Neg");
            negBlock1.Spots.Add(new Spot(54275, false));
            negBlock1.Spots.Add(new Spot(51941, false));
            negBlock1.Spots.Add(new Spot(64986, false));
            negBlock1.Mean = 57067.33;


            Nplicate posBlock2 = new Nplicate("K20");
            posBlock2.Spots.Add(new Spot(25659160, false));
            posBlock2.Spots.Add(new Spot(25164352, false));
            posBlock2.Spots.Add(new Spot(26045180, false));
            posBlock2.Mean = 25622897.33;


            Nplicate negBlock2 = new Nplicate("CD63");
            negBlock2.Spots.Add(new Spot(46504, false));
            negBlock2.Spots.Add(new Spot(41517, false));
            negBlock2.Spots.Add(new Spot(49963, false));
            negBlock2.Mean = 45994.67;

            Nplicate posBlock3 = new Nplicate("K20");
            posBlock3.Spots.Add(new Spot(24920096, true));
            posBlock3.Spots.Add(new Spot(24697788, true));
            posBlock3.Spots.Add(new Spot(25261720, true));
            posBlock3.Mean = 0;

            Nplicate negBlock3 = new Nplicate("Neg");
            negBlock3.Spots.Add(new Spot(54065, false));
            negBlock3.Spots.Add(new Spot(60231, false));
            negBlock3.Spots.Add(new Spot(161600, false));
            negBlock3.Mean = 91965.33;

            yield return new object[] { 0.99762847201790206, posBlock1, negBlock1, testBlock1};
            yield return new object[] { 0.99820493875428562, posBlock2, negBlock2, testBlock2};
            yield return new object[] { double.NaN, posBlock3, negBlock3, testBlock3};

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
