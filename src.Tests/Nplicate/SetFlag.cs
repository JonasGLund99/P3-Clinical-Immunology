using src.Data;
using System.Collections;
using Xunit;
namespace src.Tests;

public class SetFlagTest
{
    [Theory]
    [ClassData(typeof(SetFlagTestData))]
    public void SetFlag_Flags_Nplicate(bool expected, params Nplicate[] nplicates)
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
            n3.Spots.Add(new Spot(0, false));
            n3.Spots.Add(new Spot(0, false));
            n3.Spots.Add(new Spot(0, false));

            Nplicate n4 = new Nplicate("c69");
            n4.Spots.Add(new Spot(0, false));
            n4.Spots.Add(new Spot(0, false));
            n4.Spots.Add(new Spot(0, false));
            n4.Spots.Add(new Spot(0, false));

            Nplicate n5 = new Nplicate("c69");
            n5.Spots.Add(new Spot(0, false));
            n5.Spots.Add(new Spot(0, true));



            yield return new object[] { true, n1 };
            yield return new object[] { true, n2 };
            yield return new object[] { false, n3 };
            yield return new object[] { false, n4 };
            yield return new object[] { true, n5 };



        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
