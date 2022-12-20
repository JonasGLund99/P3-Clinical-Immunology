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
            await DatabaseService.Instance.Client.GetDatabase("ClinicalImmunology").DeleteAsync();
        }
        catch { }

        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();

        // Act

        // Assert
        await Assert.ThrowsAnyAsync<CosmosException>(() => DatabaseService.Instance.Client.CreateDatabaseAsync("ClinicalImmunology", 1000));

        Database database = DatabaseService.Instance.Client.GetDatabase("ClinicalImmunology");
        await Assert.ThrowsAnyAsync<CosmosException>(() => database.CreateContainerAsync("Experiment", "/PartitionKey"));
        await Assert.ThrowsAnyAsync<CosmosException>(() => database.CreateContainerAsync("ClinicalTest", "/PartitionKey"));
        await Assert.ThrowsAnyAsync<CosmosException>(() => database.CreateContainerAsync("Block", "/PartitionKey"));
        
        // Cleanup
        await database.DeleteAsync();
    }
}