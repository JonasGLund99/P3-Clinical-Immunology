using src.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xunit;
namespace src.Tests.old;

public class SetHeatMapColourTest
{
    private readonly Nplicate _sut;

    public SetHeatMapColourTest()
    {
        _sut = new Nplicate();
    }

    [Theory]
    [ClassData(typeof(SetHeatMapColourTestData))]
    public void lerpTheory(Color expected, double minRI, double maxRI, params Nplicate[] nplicates)
    {
        Color output = default;

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
            Color expectedColour1 = Color.FromArgb(255, 32, 140, 141);
            Color expectedColour2 = Color.FromArgb(255, 32, 140, 141);

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



            yield return new object[] { expected1, minRI, maxRI, n1 };
            yield return new object[] { expected1, minRI, maxRI, n2 };

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
