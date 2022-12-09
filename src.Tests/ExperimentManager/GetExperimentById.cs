using Microsoft.Azure.Cosmos;
using src.Data;
using Xunit;
namespace src.Tests;

//https://youtu.be/2Wp8en1I9oQ?t=1152 Se delen om Reusing Instances
public class GetExperimentByIdTest
{
    [Fact]
    public async void GetExperimentByIdReturnsExperiment()
    {
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for GetExperimentByIdtest in ExperimentManager");

        Container experimentContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("Experiment", "/PartitionKey");

        string guidExperiment1 = Guid.NewGuid().ToString();
        Experiment e1 = new Experiment { id = guidExperiment1, PartitionKey = guidExperiment1 };

        await experimentContainer.UpsertItemAsync<Experiment>(
                    item: e1,
                    partitionKey: new PartitionKey(e1.PartitionKey)
                );

        // Act

        await ExperimentManager.GetExperimentById(guidExperiment1);

        Assert.True(true);

        await experimentContainer.DeleteContainerAsync();
    }

    [Fact]
    public async void GetExperimentByIdNonExistingIDReturnsNull()
    {
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for SaveToDatabase test in ClinicalTest");

        Container experimentContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("Experiment", "/PartitionKey");

        string guidExperiment1 = Guid.NewGuid().ToString();
        Experiment e1 = new Experiment { id = guidExperiment1, PartitionKey = guidExperiment1 };

        // Act
        Experiment? actual = await ExperimentManager.GetExperimentById(guidExperiment1);

        Assert.Null(actual);

        await experimentContainer.DeleteContainerAsync();
    }

}


