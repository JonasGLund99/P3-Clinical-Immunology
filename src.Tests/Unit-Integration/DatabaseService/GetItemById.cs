using Xunit;
using Microsoft.Azure.Cosmos;
using src.Data;

namespace src.Tests;

public class GetItemByIdTest
{
    [Fact]
    public async void GetItemNotNull()
    {
        // Arrange
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database is null");

        Container container = await DatabaseService.Instance.Database.CreateContainerAsync("TestItem", "/PartitionKey");
        TestItem testItem = new TestItem(Guid.NewGuid().ToString());
        await container.UpsertItemAsync<TestItem>(testItem, new PartitionKey(testItem.PartitionKey));

        // Act
        TestItem? testItemFromDatabase = await DatabaseService.Instance.GetItemById<TestItem>(testItem.id, testItem.PartitionKey);

        // Assert
        Assert.NotNull(testItemFromDatabase);

        // Clean up
        await container.DeleteContainerAsync();
    }

    [Fact]
    public async void GetItemNull()
    {
        // Arrange
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database is null");

        Container container = await DatabaseService.Instance.Database.CreateContainerAsync("TestItem", "/PartitionKey");
        TestItem testItem = new TestItem(Guid.NewGuid().ToString());
        await container.UpsertItemAsync<TestItem>(testItem, new PartitionKey(testItem.PartitionKey));

        // Act
        TestItem? testItemFromDatabase = await DatabaseService.Instance.GetItemById<TestItem>(testItem.id + "test", testItem.PartitionKey);

        // Assert
        Assert.Null(testItemFromDatabase);

        // Clean up
        await container.DeleteContainerAsync();
    }
}