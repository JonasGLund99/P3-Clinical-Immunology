using src.Data;
using System.Collections;
using System.Drawing;
using Xunit;
namespace src.Tests;

public class SetHeatMapColourTest
{
    private readonly Nplicate _sut;

    public SetHeatMapColourTest()
    {
        _sut = new Nplicate();
    }

    [Theory]
    [ClassData(typeof(SetHeatMapColourTestData))]
    public void SetHeatMapColourTheory(Color expected, double minRI, double maxRI, params Nplicate[] nplicates)
    {
        foreach (Nplicate np in nplicates)
        {
            np.SetHeatMapColour(minRI, maxRI);
            _sut.HeatmapColour = np.HeatmapColour;
        }

        Assert.Equal(expected, _sut.HeatmapColour);
    }

    public class SetHeatMapColourTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            Color expectedColour1 = Color.FromArgb(255, 35, 133, 140);
            Color expectedColour2 = Color.FromArgb(255, 68, 1, 88);
            Color expectedColour3 = Color.FromArgb(255, 60, 81, 138);
            Color expectedColour4 = Color.FromArgb(255, 46, 110, 139);



            double minRI = 0.8;
            double maxRI = 8.4;

            Nplicate n1 = new Nplicate("PP13");
            n1.RI = 4.8;
            n1.Spots.Add(new Spot(22278, true));
            n1.Spots.Add(new Spot(19855, false));
            n1.Spots.Add(new Spot(24627, false));

            Nplicate n2 = new Nplicate("c69");
            n2.RI = 8.4;
            n2.Spots.Add(new Spot(0, true));
            n2.Spots.Add(new Spot(0, true));
            n2.Spots.Add(new Spot(0, true));

            Nplicate n3 = new Nplicate("c69");
            n3.RI = 6.4;
            n3.Spots.Add(new Spot(0, true));
            n3.Spots.Add(new Spot(0, true));
            n3.Spots.Add(new Spot(19855, false));
            n3.Spots.Add(new Spot(24627, false));


            Nplicate n4 = new Nplicate("c69");
            n4.RI = 5.5;
            n4.Spots.Add(new Spot(0, true));
            n4.Spots.Add(new Spot(24627, false));



            yield return new object[] { expectedColour1, minRI, maxRI, n1 };
            yield return new object[] { expectedColour2, minRI, maxRI, n2 };
            yield return new object[] { expectedColour3, minRI, maxRI, n3 };
            yield return new object[] { expectedColour4, minRI, maxRI, n4 };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
