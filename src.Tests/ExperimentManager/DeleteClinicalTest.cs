using Xunit;
using Microsoft.Azure.Cosmos;
using src.Data;

namespace src.Tests;

public class DeleteClinicalTestTest
{
    [Fact]
    public async void DeleteClinicalTestThrowsCosmosException()
    {
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for QueryExperiemnts test in Experiment Manager");

        Container experimentContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("Experiment", "/PartitionKey");

        Container clinicalTestContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("ClinicalTest", "/PartitionKey");

        object[] objects = await mockExperimentMultipleAssociates();
        Experiment experiment1 = (Experiment)objects[0];
        List<ClinicalTest> clinicalTests = new List<ClinicalTest>();

        for(int i = 0; i < 3; i++)
        {
            ClinicalTest clinicalTest = ((List<ClinicalTest>)objects[1])[i];
            clinicalTests.Add(clinicalTest);
        }

        await experiment1.SaveToDatabaseAsync();

        
        await ExperimentManager.DeleteClinicalTest(clinicalTests[0]);

        await Assert.ThrowsAnyAsync<CosmosException>(() => clinicalTestContainer.ReadItemAsync<ClinicalTest>(clinicalTests[0].id, new PartitionKey(clinicalTests[0].id)));

        await experimentContainer.DeleteContainerAsync();

        await clinicalTestContainer.DeleteContainerAsync();
    }

    [Fact]
    public async void DeleteClinicalTestRemovesIdFromExperiment()
    {
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for QueryExperiemnts test in Experiment Manager");

        Container experimentContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("Experiment", "/PartitionKey");

        Container clinicalTestContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("ClinicalTest", "/PartitionKey");

        object[] objects = await mockExperimentMultipleAssociates();
        Experiment experiment1 = (Experiment)objects[0];
        List<ClinicalTest> clinicalTests = new List<ClinicalTest>();

        for (int i = 0; i < 3; i++)
        {
            ClinicalTest clinicalTest = ((List<ClinicalTest>)objects[1])[i];
            clinicalTests.Add(clinicalTest);
        }

        await experiment1.SaveToDatabaseAsync();


        await ExperimentManager.DeleteClinicalTest(clinicalTests[0]);

        Experiment actualExperiment = await experimentContainer.ReadItemAsync<Experiment>(experiment1.id, new PartitionKey(experiment1.id));

        Assert.Null(actualExperiment.ClinicalTestIds.Find(id => id == clinicalTests[0].id));

        await experimentContainer.DeleteContainerAsync();

        await clinicalTestContainer.DeleteContainerAsync();
    }

    private async Task<object[]> mockExperimentMultipleAssociates()
    {
        string guidExperiment = Guid.NewGuid().ToString();
        string guidExperiment1 = Guid.NewGuid().ToString();
        Experiment experiment = new Experiment { id = guidExperiment, PartitionKey = guidExperiment };

        Experiment experiment1 = new Experiment { id = guidExperiment1, PartitionKey = guidExperiment1 };

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
            await ExperimentManager.Associate(experiment1, ct);
            await ct.SaveToDatabaseAsync();
        }

        return new object[] { experiment, clinicalTests };
    }

}