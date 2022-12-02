using src.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xunit;
namespace src.Tests.old;

public class CalculateRITest
{
    private readonly Nplicate _sut;

    public CalculateRITest()
    {
        _sut = new Nplicate();
    }

    [Theory]
    [ClassData(typeof(CalculateRITestData))]
    public void CalculateRITheory(double expected, params Nplicate[] nplicates)
    {
        foreach (Nplicate np in nplicates)
        {

            np.CalculateMean();
            _sut.Mean = np.Mean;
        }

        Assert.Equal(expected, _sut.Mean);
    }

    public class CalculateRITestData : IEnumerable<object[]>
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


            yield return new object[] { 22241, n1 };
            yield return new object[] { 0, n2 };

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
