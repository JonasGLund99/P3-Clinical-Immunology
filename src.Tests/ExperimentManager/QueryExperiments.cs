using Xunit;
using Microsoft.Azure.Cosmos;
using src.Data;
using System.Collections;

namespace src.Tests;

public class QueryExperimentsTest
{
    [Theory]
    [ClassData(typeof(SearchByBarcodeOrTitleData))]
    public async void SearchByExperimentNumberAuthorOrTitle(string searchParameter, int expectedNumResults)
    {
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for QueryExperiemnts test in Experiment Manager");

        Container experimentContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("Experiment", "/PartitionKey");
        Experiment experiment1 = mockExperiment();
        experiment1.Title = "Title";
        experiment1.Author = "Jørn Christas";
        experiment1.ExperimentNumber = "JE1903";
        await experiment1.SaveToDatabaseAsync();

        Experiment experiment2 = mockExperiment();
        experiment2.Title = "TestTitle";
        experiment2.Author = "René";
        experiment2.ExperimentNumber = "JE1902";
        await experiment2.SaveToDatabaseAsync();

        Experiment experiment3 = mockExperiment();
        experiment3.Title = "anothertesttitle";
        experiment3.Author = "Papa Noël";
        experiment3.ExperimentNumber = "FA7246";
        await experiment3.SaveToDatabaseAsync();

        Experiment experiment4 = mockExperiment();
        experiment4.Title = "IHaveSpecialCharacters æblemost";
        experiment4.Author = "Jørn";
        experiment4.ExperimentNumber = "GE7247";
        await experiment4.SaveToDatabaseAsync();

        Experiment experiment5 = mockExperiment();
        experiment5.Title = "gsjdfs";
        experiment5.Author = "farooq";
        experiment5.ExperimentNumber = "KL6432";
        await experiment5.SaveToDatabaseAsync();

        List<Experiment> experiments = await ExperimentManager.QueryExperiments(searchParameter);
        
        await experimentContainer.DeleteContainerAsync();
        
        Assert.Equal(expectedNumResults, experiments.Count);


    }
    class SearchByBarcodeOrTitleData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            // Cases for querying after Experiment.Title
            yield return new object[] { "Title", 3 };
            yield return new object[] { "TestTitle", 2 };
            yield return new object[] { "anothertesttitle", 1 };
            yield return new object[] { "IHaveSpecialCharacters Æblemost", 1 };
            yield return new object[] { "not existing", 0 };

            // Cases for querying after Experiment.Author
            yield return new object[] { "ë", 1 };
            yield return new object[] { "Jørn", 2 };
            yield return new object[] { "Jørn Christas", 1 };
            yield return new object[] { "Papa Noël", 1 };
            yield return new object[] { "Christina Stürmer", 0 };

            // Cases for querying after Experiment.ExperimentNumber
            yield return new object[] { "JE190", 2 };
            yield return new object[] { "FA7246", 1 };
            yield return new object[] { "FA", 2 };
            yield return new object[] { "ge7247", 1 };
            yield return new object[] { "je99999999", 0 };


        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    private Experiment mockExperiment()
    {
        string guidExperiment = Guid.NewGuid().ToString();
        Experiment experiment = new Experiment { id = guidExperiment, PartitionKey = guidExperiment };
        List<ClinicalTest> clinicalTests = new List<ClinicalTest>
        {
            new ClinicalTest(Guid.NewGuid().ToString()),
            new ClinicalTest(Guid.NewGuid().ToString()),
            new ClinicalTest(Guid.NewGuid().ToString()),
        };

        return experiment;
    }
}