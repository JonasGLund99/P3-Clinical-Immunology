using src.Data;
using System.Collections;
using Xunit;
namespace src.Tests;

public class CalculateMeanTest
{
    private readonly Nplicate _sut;

    public CalculateMeanTest()
    {
        _sut = new Nplicate();
    }

    [Theory]
    [ClassData(typeof(CalculateMeanTestData))]
    public void CalculateMeanTheory(double expected, params Nplicate[] nplicates)
    {
        foreach (Nplicate np in nplicates)
        {
            np.CalculateMean();
            _sut.Mean = np.Mean;
        }

        Assert.Equal(expected, _sut.Mean);
    }

    public class CalculateMeanTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            Nplicate n1 = new Nplicate("PP13");
            n1.Spots.Add(new Spot(22278, true));
            n1.Spots.Add(new Spot(19855, false));
            n1.Spots.Add(new Spot(24627, false));


            Nplicate n2 = new Nplicate("c69");
            n2.Spots.Add(new Spot(0, true));
            n2.Spots.Add(new Spot(0, true));
            n2.Spots.Add(new Spot(0, true));

            Nplicate n3 = new Nplicate("c69");
            n3.Spots.Add(new Spot(19855, false));
            n3.Spots.Add(new Spot(24627, false));
            n3.Spots.Add(new Spot(23000, false));
            n3.Spots.Add(new Spot(22278, false));

            Nplicate n4 = new Nplicate("c69");
            n4.Spots.Add(new Spot(19855, false));
            n4.Spots.Add(new Spot(24627, false));

            yield return new object[] { 22241, n1 };
            yield return new object[] { 0, n2 };
            yield return new object[] { 22440, n3};
            yield return new object[] { 22241, n4 };


        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
