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

public class GetFlagCountTest
{

    [Theory]
    [ClassData(typeof(GetFlagCountTestData))]
    public void GetFlagCount_Finds_Correct_Flag_Count(int expected, Nplicate nplicate)
    {
        Assert.Equal(expected, nplicate.GetFlagCount());
    }

    public class GetFlagCountTestData : IEnumerable<object[]>
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
            n3.Spots.Add(new Spot(0, false));
            n3.Spots.Add(new Spot(0, true));
            n3.Spots.Add(new Spot(0, true));

            Nplicate n4 = new Nplicate("c69");
            n4.Spots.Add(new Spot(0, true));
            n4.Spots.Add(new Spot(0, true));
            n4.Spots.Add(new Spot(0, true));
            n4.Spots.Add(new Spot(0, true));

            Nplicate n5 = new Nplicate("c69");
            n5.Spots.Add(new Spot(0, true));
            n5.Spots.Add(new Spot(0, true));

            yield return new object[] { 1, n1 };
            yield return new object[] { 3, n2 };
            yield return new object[] { 2, n3 };
            yield return new object[] { 4, n4 };
            yield return new object[] { 2, n5 };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
