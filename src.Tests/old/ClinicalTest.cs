// using Microsoft.Azure.Cosmos;
// using src.Data;
// using System.Collections;
// using Xunit;

// namespace Tests;


// //Inspiration source: https://www.youtube.com/watch?v=2Wp8en1I9oQ;
// public class ClinicalTestTests
// {
//     //Ikke hate det betyder system under test.
//     private readonly ClinicalTest _sut;

//     public ClinicalTestTests()
//     {
//         _sut = new ClinicalTest();
//     }

//     [Theory]
//     [ClassData(typeof(ClinicalTestTestData))]
//     public void CalculateClinicalTestResultTheory(params ClinicalTest[] clinicalTests)
//     {
//     }

//     public class ClinicalTestTestData : IEnumerable<object[]>
//     {

//         public IEnumerator<object[]> GetEnumerator()
//         {
//             yield return new object[] { generateClinicalTest() };
//         }

//         IEnumerator IEnumerable.GetEnumerator()
//         {
//             return GetEnumerator();
//         }

//         private ClinicalTest generateClinicalTest()
//         {
//             List<SlideDataFile> slideDataFiles = new List<SlideDataFile> {
//                 new SlideDataFile(
//                     filename: "10000465_0016_flag.txt",
//                     content: File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data/MockedData/10000465_0016_flag.txt"))
//                 ),
//                 new SlideDataFile(
//                     filename: "10000466_0014_flag.txt",
//                     content: File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data/MockedData/10000466_0014_flag.txt"))
//                 ),
//                 new SlideDataFile(
//                     filename: "10000467_0005_flag.txt",
//                     content: File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data/MockedData/10000467_0005_flag.txt"))
//                 )
//             };

//             ClinicalTest ct = new ClinicalTest(
//                 id: Guid.NewGuid().ToString(),
//                 title: $"Test 1 in TESTEXP-1",
//                 nplicateSize: 3,
//                 description: "Some description",
//                 createdAt: DateTime.Now
//             );
//             ct.SlideDataFiles = slideDataFiles;

//             ct.Matches = new Dictionary<string, int>()
//             {
//                 { slideDataFiles[0].Filename, 0 },
//                 { slideDataFiles[1].Filename, 1 },
//                 { slideDataFiles[2].Filename, 2 }
//             };
//             ct.TableTitles = new List<string>() { "key1", "key2", "key3", "key4", "key5" };
//             ct.ChosenTableTitles = new string[] { "key2", "key1", "key3" };

//             return ct;

//         }
//     }

//     //[Fact]
//     //public void Test_CreatePatientKeys()
//     //{
//     //    ClinicalTest ct = new ClinicalTest(id: "1");

//     //    ct.CreatePatientKeys(
//     //        allKeys: new List<string>() { "age", "SoT", "isDead", "height" },
//     //        "age",
//     //        "height"
//     //    );

//     //    Assert.True(ct.PatientKeys["age"] && ct.PatientKeys["height"] && !ct.PatientKeys["SoT"] && !ct.PatientKeys["isDead"]);
//     //}

// }
