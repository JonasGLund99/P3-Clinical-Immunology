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
namespace src.Tests;

public class SetFlagTest
{
    private readonly Nplicate _sut;

    public SetFlagTest()
    {
        _sut = new Nplicate();
    }

    [Theory]
    [ClassData(typeof(SetFlagTestData))]
    public void CalculateRITheory(bool expected, params Nplicate[] nplicates)
    {
        bool isFlagged = false;
        foreach (Nplicate np in nplicates)
        {

            np.SetFlag();
            isFlagged = np.IsFlagged;
        }

        Assert.Equal(expected, isFlagged);
    }

    public class SetFlagTestData: IEnumerable<object[]>
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
            n2.Spots.Add(new Spot(0, false));
            n2.Spots.Add(new Spot(0, false));
            n2.Spots.Add(new Spot(0, false));


            yield return new object[] { true, n1 };
            yield return new object[] { true, n2 };
            yield return new object[] { false, n3 };

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
