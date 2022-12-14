//using src.Data;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.Linq;
//using System.Security.Cryptography.X509Certificates;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;
//namespace src.Tests;

//public class lerpTest
//{
//    private readonly Nplicate _sut;

//    public lerpTest()
//    {
//        _sut = new Nplicate();
//    }

//    [Theory]
//    [ClassData(typeof(fixerTestData))]
//    public void lerpTheory(double expected, Color low, Color medium, double weight, params Nplicate[] nplicates)
//    {


//        Color output = default;

//        foreach (Nplicate np in nplicates)
//        {
//            np.fixer(low, medium, (weight - 0.25) * 4));
//            _sut.Mean = np.Mean;
//        }

//        Assert.Equal(expected, _sut.Mean);
//    }

//    public class fixerTestData : IEnumerable<object[]>
//    {
//        public IEnumerator<object[]> GetEnumerator()
//        {
//            Color max = Color.FromArgb(255, 249, 235, 46);
//            Color high = Color.FromArgb(255, 76, 194, 108);
//            Color medium = Color.FromArgb(255, 32, 140, 141);
//            Color low = Color.FromArgb(255, 62, 78, 138);
//            Color min = Color.FromArgb(255, 68, 1, 88);

//            Color expected1 = Color.FromArgb(255, 32, 140, 141);

//            double minRI = 0.8;
//            double maxRI = 8.4;
//            double weight = (5 - minRI) / (maxRI - minRI);

//            Nplicate n1 = new Nplicate("PP13");
//            n1.Spots.Add(new Spot(22278, true));
//            n1.Spots.Add(new Spot(19855, false));
//            n1.Spots.Add(new Spot(24627, false));


//            Nplicate n2 = new Nplicate("c69");
//            n2.Spots.Add(new Spot(0, true));
//            n2.Spots.Add(new Spot(0, true));
//            n2.Spots.Add(new Spot(0, true));


//            yield return new object[] { expected1, low, medium, weight, n1 };
//            yield return new object[] { expected1, low, medium, weight, n2 };

//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }
//    }

//}
