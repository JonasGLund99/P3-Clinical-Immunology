using Xunit;
using Microsoft.Azure.Cosmos;
using src.Data;

namespace src.Tests;

public class SaveToDatabaseAsyncTest
{
    [Fact]
    public async void SaveItem()
    {
        // Arrange
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database is null");

        Container container = await DatabaseService.Instance.Database.CreateContainerAsync("TestItem", "/id");
        TestItem item = new TestItem(Guid.NewGuid().ToString());
    
        // Act
        await item.SaveToDatabaseAsync();
        TestItem itemFromDatabase = await container.ReadItemAsync<TestItem>(item.id, new PartitionKey(item.id));
    
        // Assert
        Assert.True(item.id == itemFromDatabase.id);

        // Clean up
        await container.DeleteContainerAsync();
    }

    [Fact]
    public async void SaveItemCatchException() // Container name doesn't match type name
    {
        // Arrange
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database is null");

        Container container = await DatabaseService.Instance.Database.CreateContainerAsync("TestItemExceptionTest", "/id");
        TestItem item = new TestItem(Guid.NewGuid().ToString());
    
        // Act
        await item.SaveToDatabaseAsync();

        // Assert
        Assert.True(true);

        // Clean up
        await container.DeleteContainerAsync();
    }
}