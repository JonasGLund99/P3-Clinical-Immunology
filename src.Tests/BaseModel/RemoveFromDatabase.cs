using Xunit;
using Microsoft.Azure.Cosmos;
using src.Data;

namespace src.Tests;

public class RemoveFromDatabaseAsyncTest
{
    [Fact]
    public async void RemoveItem()
    {
        // Arrange
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database is null");

        Container container = await DatabaseService.Instance.Database.CreateContainerAsync("TestItem", "/id");
        string testId = Guid.NewGuid().ToString();
        TestItem item = new TestItem(testId);

        // Act
        await container.UpsertItemAsync<TestItem>(item, new PartitionKey(item.id));
        await item.RemoveFromDatabase();
    
        // Assert
        await Assert.ThrowsAnyAsync<CosmosException>(() => container.ReadItemAsync<TestItem>(testId, new PartitionKey(item.id)));

        // Clean up
        await container.DeleteContainerAsync();
    }

    [Fact]
    public async void RemoveItemCatchException() // Container name doesn't match type name
    {
        // Arrange
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database is null");

        Container container = await DatabaseService.Instance.Database.CreateContainerAsync("TestItemExceptionTest", "/id");
        TestItem item = new TestItem(Guid.NewGuid().ToString());
    
        // Act
        await container.UpsertItemAsync<TestItem>(item, new PartitionKey(item.id));
        await item.RemoveFromDatabase();

        // Assert
        Assert.True(true);

        // Clean up
        await container.DeleteContainerAsync();
    }
}

