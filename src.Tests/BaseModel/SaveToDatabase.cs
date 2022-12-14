using Xunit;
using Microsoft.Azure.Cosmos;
using src.Data;

namespace src.Tests;

public class SaveToDatabaseTest
{
    [Fact]
    public async void QueueIsRunningAfterSave()
    {
        // Arrange
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database is null");

        Container container = await DatabaseService.Instance.Database.CreateContainerAsync("TestItemBlock", "/PartitionKey");
        string id = Guid.NewGuid().ToString();
        TestItem block = new TestItem(id);

        // Act
        ProcessQueue.Instance.Clear();
        block.SaveToDatabase();

        // Assert
        Assert.True(ProcessQueue.Instance.IsRunning);

        // Clean up
        await container.DeleteContainerAsync();
    }
}