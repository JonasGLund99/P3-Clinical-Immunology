// using src.Data;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.ComponentModel;
// using System.Drawing;
// using System.Linq;
// using System.Security.Cryptography.X509Certificates;
// using System.Text;
// using System.Threading.Tasks;
// using Xunit;
// namespace Tests;

// public class NplicateTest
// {
//     private readonly Nplicate _sut;

//     public NplicateTest()
//     {
//         _sut = new Nplicate();
//     }

//     [Theory]
//     [ClassData(typeof(NplicateTestData))]
//     public void CalculateMeanTheory(double expected, params Nplicate[] nplicates)
//     {
//         foreach(Nplicate np in nplicates)
//         {

//             np.CalculateMean();
//             _sut.Mean = np.Mean;
//         }

//         Assert.Equal(expected, _sut.Mean); 
//     }

//     [Theory]
//     [MemberData(nameof(SetHeatMapColourTheoryData)), 
//     ClassData(typeof(NplicateTestData))]
//     //https://youtu.be/2Wp8en1I9oQ?t=461
//     public void SetHeatMapColourTheory(Color expected, params Nplicate[] nplicates)
//     {
//         double maxRI = 8.05;
//         double minRI = 0;

//         foreach (Nplicate np in nplicates)
//         {
//             np.SetHeatMapColour(maxRI, minRI);

//             _sut.HeatmapColour = np.HeatmapColour;
//         }

//         Assert.Equal(expected, _sut.HeatmapColour);

//     }

//     public static IEnumerable<object[]> SetHeatMapColourTheoryData()
//     {
//         Color max = Color.FromArgb(255, 249, 235, 46);
//         Color high = Color.FromArgb(255, 76, 194, 108);
//         Color medium = Color.FromArgb(255, 32, 140, 141);
//         Color low = Color.FromArgb(255, 62, 78, 138);
//         Color min = Color.FromArgb(255, 68, 1, 88);
//         Color expected1 = Color.FromArgb((int)255, (int)50, (int)80, (int)100);
//         Color expected2 = Color.FromArgb((int)255, (int)50, (int)80, (int)100);


//         yield return new object[] { expected1 };
//         yield return new object[] { expected2 };

//     }



//     public class NplicateTestData : IEnumerable<object[]>
//     {
//         public IEnumerator<object[]> GetEnumerator()
//         {
//             Nplicate n1 = new Nplicate("PP13");
//             n1.Spots.Add(new Spot(22278, true));
//             n1.Spots.Add(new Spot(19855, false));
//             n1.Spots.Add(new Spot(24627, false));


//             Nplicate n2 = new Nplicate("c69");
//             n2.Spots.Add(new Spot(0, true));
//             n2.Spots.Add(new Spot(0, true));
//             n2.Spots.Add(new Spot(0, true));


//             yield return new object[] { 22241, n1 };
//             yield return new object[] { 0, n2 };

//         }

//         IEnumerator IEnumerable.GetEnumerator()
//         {
//             return GetEnumerator();
//         }
//     }

// }
