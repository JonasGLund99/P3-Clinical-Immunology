using Xunit;
using Microsoft.Azure.Cosmos;
using src.Data;

namespace src.Tests;

public class SaveToDatabaseAsyncTest
{
    [Fact]
    public async void SaveSingleItem()
    {
        // Arrange
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) return;

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
}

public class TestItem : BaseModel<TestItem>
{
    public TestItem(string id) : base(id)
    {
        PartitionKey = id;
    }

    public override string PartitionKey { get; set; }
}