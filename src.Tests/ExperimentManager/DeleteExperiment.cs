using Xunit;
using Microsoft.Azure.Cosmos;
using src.Data;

namespace src.Tests;

public class DeleteExperimentTest
{
    [Fact]
    public async void DeleteExperimentThrowsCosmosException()
    {
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for QueryExperiemnts test in Experiment Manager");

        Container experimentContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("Experiment", "/PartitionKey");

        Container clinicalTestContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("ClinicalTest", "/PartitionKey");

        Experiment experiment1 = await mockExperiment();
        experiment1.Title = "Title";
        experiment1.Author = "Jørn Christas";
        experiment1.ExperimentNumber = "JE1903";

        List<string> experiment1ClinicalTestIds = new List<string>();

        foreach (string id in experiment1.ClinicalTestIds)
        {
            experiment1ClinicalTestIds.Add(id);
        }


        await experiment1.SaveToDatabaseAsync();
        await ExperimentManager.DeleteExperiment(experiment1);

        await Assert.ThrowsAnyAsync<CosmosException>(() => experimentContainer.ReadItemAsync<Experiment>(experiment1.id, new PartitionKey(experiment1.id)));

        await Assert.ThrowsAnyAsync<CosmosException>(() => clinicalTestContainer.ReadItemAsync<ClinicalTest>(experiment1ClinicalTestIds[0], new PartitionKey(experiment1ClinicalTestIds[0])));

        await Assert.ThrowsAnyAsync<CosmosException>(() => clinicalTestContainer.ReadItemAsync<ClinicalTest>(experiment1ClinicalTestIds[1], new PartitionKey(experiment1ClinicalTestIds[1])));

        await Assert.ThrowsAnyAsync<CosmosException>(() => clinicalTestContainer.ReadItemAsync<ClinicalTest>(experiment1ClinicalTestIds[2], new PartitionKey(experiment1ClinicalTestIds[2])));


        await experimentContainer.DeleteContainerAsync();

        await clinicalTestContainer.DeleteContainerAsync();
    }

    [Fact]
    public async void DeleteExperimentDoesNotDeleteClinicalTestWithMultipleAssociatedExperiments()
    {
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for QueryExperiemnts test in Experiment Manager");

        Container experimentContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("Experiment", "/PartitionKey");

        Container clinicalTestContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("ClinicalTest", "/PartitionKey");

        Experiment experiment1 = await mockExperimentMultipleAssociates();

        List<string> experiment1ClinicalTestIds = new List<string>();

        foreach (string id in experiment1.ClinicalTestIds)
        {
            experiment1ClinicalTestIds.Add(id);
        }

        await ExperimentManager.DeleteExperiment(experiment1);

        await Assert.ThrowsAnyAsync<CosmosException>(() => experimentContainer.ReadItemAsync<Experiment>(experiment1.id, new PartitionKey(experiment1.id)));

        await clinicalTestContainer.ReadItemAsync<ClinicalTest>(experiment1ClinicalTestIds[0], new PartitionKey(experiment1ClinicalTestIds[0]));
        Assert.True(true);

        await clinicalTestContainer.ReadItemAsync<ClinicalTest>(experiment1ClinicalTestIds[1], new PartitionKey(experiment1ClinicalTestIds[1]));
        Assert.True(true);


        await clinicalTestContainer.ReadItemAsync<ClinicalTest>(experiment1ClinicalTestIds[2], new PartitionKey(experiment1ClinicalTestIds[2]));
        Assert.True(true);


        await experimentContainer.DeleteContainerAsync();

        await clinicalTestContainer.DeleteContainerAsync();
    }

    private async Task<Experiment> mockExperiment()
    {
        string guidExperiment = Guid.NewGuid().ToString();
        Experiment experiment = new Experiment { id = guidExperiment, PartitionKey = guidExperiment};

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

    private async Task<Experiment> mockExperimentMultipleAssociates()
    {
        Experiment experiment = new Experiment { id = Guid.NewGuid().ToString(), PartitionKey = Guid.NewGuid().ToString() };

        Experiment experiment1 = new Experiment { id = Guid.NewGuid().ToString(), PartitionKey = Guid.NewGuid().ToString() };

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

        return experiment;
    }

}