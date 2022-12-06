using Xunit;
using Microsoft.Azure.Cosmos;
using src.Data;

namespace src.Tests;

public class SaveToDatabaseTest
{
    [Fact]
    public async void BlockQueuedWithPartitionKey()
    {
        // Arrange
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database is null");

        Container container = await DatabaseService.Instance.Database.CreateContainerAsync("TestItemBlock", "/PartitionKey");
        string id = Guid.NewGuid().ToString();
        string partitionKey = Guid.NewGuid().ToString();
        TestItemBlock block = new TestItemBlock { id = id, PartitionKey = partitionKey };

        // Act
        block.SaveToDatabase();

        // Assert
        Assert.True(ProcessQueue.Instance.GetQueues().ContainsKey(partitionKey));
        Assert.False(ProcessQueue.Instance.GetQueues().ContainsKey(id));

        // Clean up
        await container.DeleteContainerAsync();
    }

    [Fact]
    public async void ClinicalTestQueuedWithId()
    {
        // Arrange
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database is null");

        Container container = await DatabaseService.Instance.Database.CreateContainerAsync("TestItemClinicalTest", "/PartitionKey");
        string id = Guid.NewGuid().ToString();
        string partitionKey = Guid.NewGuid().ToString();
        TestItemClinicalTest clinicalTest = new TestItemClinicalTest { id = id, PartitionKey = partitionKey };

        // Act
        clinicalTest.SaveToDatabase();

        // Assert
        Assert.True(ProcessQueue.Instance.GetQueues().ContainsKey(id));
        Assert.False(ProcessQueue.Instance.GetQueues().ContainsKey(partitionKey));

        // Clean up
        await container.DeleteContainerAsync();
    }
}

public class TestItemBlock : Block { }
public class TestItemClinicalTest : ClinicalTest { }