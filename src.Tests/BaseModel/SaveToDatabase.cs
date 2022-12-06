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
        TestItemBlock block = new TestItemBlock();
        block.id = id;
        block.PartitionKey = partitionKey;

        // Act
        if (ProcessQueue.Instance.GetQueues().Count > 0) throw new Exception("Queue is not empty");
        block.SaveToDatabase();

        // Assert
        Assert.True(ProcessQueue.Instance.GetQueues().ContainsKey(partitionKey));

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
        TestItemBlock block = new TestItemBlock();
        block.id = id;
        block.PartitionKey = partitionKey;

        // Act
        if (ProcessQueue.Instance.GetQueues().Count > 0) throw new Exception("Queue is not empty");
        block.SaveToDatabase();

        // Assert
        Assert.True(ProcessQueue.Instance.GetQueues().ContainsKey(partitionKey));

        // Clean up
        await container.DeleteContainerAsync();
    }
}

public class TestItemBlock : Block { }
public class TestItemClinicalTest : ClinicalTest { }