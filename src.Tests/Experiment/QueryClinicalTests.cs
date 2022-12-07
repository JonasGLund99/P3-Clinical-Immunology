using Xunit;
using Microsoft.Azure.Cosmos;
using src.Data;
using System.Collections;

namespace src.Tests;

public class QueryClinicalTestsTest
{
    [Theory]
    [ClassData(typeof(SearchByBarcodeOrTitleData))]
    public async void SearchByBarcodeOrTitle(string searchParameter, int expectedNumResults)
    {
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        Experiment experiment = await mockExperiment();

        List<ClinicalTest> clinicalTests = await experiment.QueryClinicalTests(searchParameter);

        Assert.True(clinicalTests.Count == expectedNumResults);

        await experiment.RemoveFromDatabase();
    }
    class SearchByBarcodeOrTitleData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "Test", 3 };
            yield return new object[] { "tesTbarcode", 2 }; // Case in-sensitive?
            yield return new object[] { "Barcode", 3 };
            yield return new object[] { "testbarcodetest", 1 };
            
            yield return new object[] { "Title", 3 };
            yield return new object[] { "TestTitle", 2 };
            yield return new object[] { "anothertesttitle", 1 };

            yield return new object[] { "not existing", 0 };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    async Task<Experiment> mockExperiment()
    {
        Experiment experiment = new Experiment(Guid.NewGuid().ToString());
        List<ClinicalTest> clinicalTests = new List<ClinicalTest>
        {
            new ClinicalTest(Guid.NewGuid().ToString()),
            new ClinicalTest(Guid.NewGuid().ToString()),
            new ClinicalTest(Guid.NewGuid().ToString()),
        };

        clinicalTests[0].Slides.Add(new Slide { Barcode = "TestBarcode" });
        clinicalTests[1].Slides.Add(new Slide { Barcode = "BarcodeTest" });
        clinicalTests[2].Slides.Add(new Slide { Barcode = "TestBarcodeTest" });

        clinicalTests[0].Title = "Title";
        clinicalTests[1].Title = "TestTitle";
        clinicalTests[2].Title = "AnotherTestTitle";
        
        foreach (ClinicalTest ct in clinicalTests)
        {
            int index = clinicalTests.IndexOf(ct);
            await ExperimentManager.Associate(experiment, ct);
            await ct.SaveToDatabaseAsync();
        }

        return experiment;
    }
}