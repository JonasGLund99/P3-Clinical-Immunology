using src.Data;
using System.Collections;
using Xunit;
namespace src.Tests;

public class CalculateRITest
{
    private readonly Nplicate _sut;

    public CalculateRITest()
    {
        _sut = new Nplicate();
    }

    [Theory]
    [ClassData(typeof(CalculateRITestData))]
    public void CalculateRITheory(double expected, Nplicate correspondingBlank, Nplicate neg, params Nplicate[] nplicates)
    {
        foreach (Nplicate np in nplicates)
        {
            _sut.RI = np.CalculateRI(correspondingBlank, neg);
        }

        Assert.Equal(expected, _sut.RI);
    }

    public class CalculateRITestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            Nplicate testNplicate1 = new Nplicate("CD9");
            testNplicate1.Spots.Add(new Spot(6096001, false));
            testNplicate1.Spots.Add(new Spot(7235172, false));
            testNplicate1.Spots.Add(new Spot(9251540, false));
            testNplicate1.Mean = 7527571;

            Nplicate correspondingBlank1 = new Nplicate("CD9");
            correspondingBlank1.Spots.Add(new Spot(33099, false));
            correspondingBlank1.Spots.Add(new Spot(36860, false));
            correspondingBlank1.Spots.Add(new Spot(34385, false));
            correspondingBlank1.Mean = 34781.33;

            
            Nplicate negBlock1 = new Nplicate("Neg");
            negBlock1.Spots.Add(new Spot(54275, false));
            negBlock1.Spots.Add(new Spot(51941, false));
            negBlock1.Spots.Add(new Spot(64986, false));
            negBlock1.Mean = 57067.33;


            Nplicate testNplicate2 = new Nplicate("CD63");
            testNplicate2.Spots.Add(new Spot(369730, true));
            testNplicate2.Spots.Add(new Spot(294669, false));
            testNplicate2.Spots.Add(new Spot(444791, false));
            testNplicate2.Mean = 369730;


            Nplicate correspondingBlank2 = new Nplicate("CD63");
            correspondingBlank2.Spots.Add(new Spot(26546, false));
            correspondingBlank2.Spots.Add(new Spot(29825, false));
            correspondingBlank2.Spots.Add(new Spot(28318, false));
            correspondingBlank2.Mean = 28229.67;


            Nplicate testNplicate3 = new Nplicate("CD81");
            testNplicate3.Spots.Add(new Spot(369730, true));
            testNplicate3.Spots.Add(new Spot(294669, false));
            testNplicate3.Spots.Add(new Spot(444791, false));
            testNplicate3.Mean = 369730;


            Nplicate negBlock2 = new Nplicate("Neg");
            negBlock2.Spots.Add(new Spot(54275, true));
            negBlock2.Spots.Add(new Spot(51941, true));
            negBlock2.Spots.Add(new Spot(64986, true));
            negBlock2.Mean = 0;


            Nplicate correspondingBlank3 = new Nplicate("CD81");
            correspondingBlank3.Spots.Add(new Spot(26546, false));
            correspondingBlank3.Spots.Add(new Spot(29825, false));
            correspondingBlank3.Spots.Add(new Spot(28318, false));
            correspondingBlank3.Mean = 28229.67;






            Nplicate testNplicate4 = new Nplicate("BD9");
            testNplicate4.Spots.Add(new Spot(6096001, false));
            testNplicate4.Spots.Add(new Spot(7235172, false));
            testNplicate4.Spots.Add(new Spot(9251540, false));
            testNplicate4.Spots.Add(new Spot(7235172, false));
            testNplicate4.Mean = 7454471.25;

            Nplicate correspondingBlank4 = new Nplicate("BD9");
            correspondingBlank4.Spots.Add(new Spot(33099, false));
            correspondingBlank4.Spots.Add(new Spot(36860, false));
            correspondingBlank4.Spots.Add(new Spot(34385, false));
            correspondingBlank4.Spots.Add(new Spot(34385, false));
            correspondingBlank4.Mean = 34682.25;


            Nplicate negBlock4 = new Nplicate("Neg");
            negBlock4.Spots.Add(new Spot(54275, false));
            negBlock4.Spots.Add(new Spot(51941, false));
            negBlock4.Spots.Add(new Spot(64986, false));
            negBlock4.Spots.Add(new Spot(64986, false));
            negBlock4.Mean = 59047;

            //1   2   1   CD63 CD63    369730 - 100
            //1   2   2   CD63 CD63    294669  0
            //1   2   3   CD63 CD63    444791  0

            //1   1   4   Blank neg 54275   0
            //1   1   5   Blank neg 51941   0
            //1   1   6   Blank neg 64986   0

            //21  2   1   CD63 CD63    26546   0
            //21  2   2   CD63 CD63    29825   0
            //21  2   3   CD63 CD63    28318   0

            yield return new object[] { 7.0366940784893153, correspondingBlank1, negBlock1, testNplicate1 };
            yield return new object[] { 2.581150002062131, correspondingBlank2, negBlock1, testNplicate2 };
            yield return new object[] { double.NaN, correspondingBlank3, negBlock2, testNplicate3 };
            yield return new object[] { 6.973370588112287, correspondingBlank4, negBlock4, testNplicate4 };


        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
