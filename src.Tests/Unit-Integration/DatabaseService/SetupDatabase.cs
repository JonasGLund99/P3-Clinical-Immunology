using Xunit;
using Microsoft.Azure.Cosmos;
using src.Data;

namespace src.Tests;

public class SetupDatabaseTest
{
    [Fact]
    public async void DatabaseAndContainersExist()
    {
        // Arrange
        try
        {
            await DatabaseService.Instance.Client.GetDatabase("ClinicalImmunology2").DeleteAsync();
        }
        catch { }

        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();

        // Act

        // Assert
        await Assert.ThrowsAnyAsync<CosmosException>(() => DatabaseService.Instance.Client.CreateDatabaseAsync("ClinicalImmunology2", 1000));

        Database database = DatabaseService.Instance.Client.GetDatabase("ClinicalImmunology2");
        await Assert.ThrowsAnyAsync<CosmosException>(() => database.CreateContainerAsync("Experiment", "/PartitionKey"));
        await Assert.ThrowsAnyAsync<CosmosException>(() => database.CreateContainerAsync("ClinicalTest", "/PartitionKey"));
        await Assert.ThrowsAnyAsync<CosmosException>(() => database.CreateContainerAsync("Block", "/PartitionKey"));
        
        // Cleanup
        await database.DeleteAsync();
    }
}