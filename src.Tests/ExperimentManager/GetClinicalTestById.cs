using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using OfficeOpenXml.Core;
using src.Data;
using System.Collections;
using Xunit;

namespace src.Tests;

//https://youtu.be/2Wp8en1I9oQ?t=1152 Se delen om Reusing Instances
public class GetClinicalTestByIdTest
{
    [Fact]
    public async void GetClinicalTestByIdReturnsExperiment()
    {
        DatabaseService.EnableTestMode();

        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for SaveToDatabase test in ClinicalTest");

        Container clinicalTestContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("ClinicalTest", "/PartitionKey");

        string guidClinicalTest1 = Guid.NewGuid().ToString();
        ClinicalTest c1 = new ClinicalTest { id = guidClinicalTest1, PartitionKey = guidClinicalTest1};

        await clinicalTestContainer.UpsertItemAsync<ClinicalTest>(
                    item: c1,
                    partitionKey: new PartitionKey(c1.PartitionKey)
                );

        // Act

        await ExperimentManager.GetExperimentById(guidClinicalTest1);

        Assert.True(true);

        await clinicalTestContainer.DeleteContainerAsync();
    }

    [Fact]
    public async void GetClinicalTestByIdNonExistingIDReturnsNull()
    {
        DatabaseService.EnableTestMode();

        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for SaveToDatabase test in ClinicalTest");

        Container clinicalTestContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("ClinicalTest", "/PartitionKey");

        string guidClinicalTest1 = Guid.NewGuid().ToString();
        ClinicalTest clinicalTest1 = new ClinicalTest { id = guidClinicalTest1, PartitionKey = guidClinicalTest1 };

        // Act
        ClinicalTest? actual = await ExperimentManager.GetClinicalTestById(guidClinicalTest1);

        Assert.Null(actual);

        await clinicalTestContainer.DeleteContainerAsync();
    }

}


