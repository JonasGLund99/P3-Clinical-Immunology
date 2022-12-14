using Microsoft.Azure.Cosmos;
using src.Data;
using Xunit;

namespace src.Tests;

//https://youtu.be/2Wp8en1I9oQ?t=1152 Se delen om Reusing Instances
public class RemoveFromDatabaseTest
{

    [Fact]
    public async void RemoveFromDatabaseRemovesClinicalTest()
    {
        string guidClinicalTest = Guid.NewGuid().ToString();
        ClinicalTestRemoveFromDatabase c2 = new ClinicalTestRemoveFromDatabase { id = guidClinicalTest, PartitionKey = guidClinicalTest };


        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for SaveToDatabase test in ClinicalTest");
        Container ctContainer = await DatabaseService.Instance.Database.CreateContainerAsync("ClinicalTestRemoveFromDatabase", "/PartitionKey");

        // Act
        c2.SaveToDatabase(true);

        while (ProcessQueue.Instance.IsRunning[guidClinicalTest])
        {

        }

        await c2.RemoveFromDatabase();

        await Assert.ThrowsAnyAsync<CosmosException>(() => ctContainer.ReadItemAsync<ClinicalTestRemoveFromDatabase>(c2.id, new PartitionKey(c2.PartitionKey)));


        // Clean up
        await ctContainer.DeleteContainerAsync();
    }

    [Fact]
    public async void RemoveFromDatabaseRemovesBlocksAndClinicalTest()
    {
        string guidNBlock = Guid.NewGuid().ToString();
        string guidBBlock = Guid.NewGuid().ToString();
        string guidClinicalTest = Guid.NewGuid().ToString();
        List<BlockClinicalTestRemoveFromDatabase> nBlocks = new List<BlockClinicalTestRemoveFromDatabase>
        {
            new BlockClinicalTestRemoveFromDatabase { SlideIndex = 0, BlockIndex = 0, Type = Block.BlockType.Normal, id = guidNBlock, PartitionKey = guidClinicalTest },
        };

        List<BlockClinicalTestRemoveFromDatabase> bBlocks = new List<BlockClinicalTestRemoveFromDatabase>
        {
            new BlockClinicalTestRemoveFromDatabase { SlideIndex = 0, BlockIndex = 0, Type = Block.BlockType.Blank, id = guidBBlock, PartitionKey = guidClinicalTest },
        };
        ClinicalTestRemoveFromDatabase c2 = new ClinicalTestRemoveFromDatabase { id = guidClinicalTest, PartitionKey = guidClinicalTest };

        await c2.AddNormalBlock(nBlocks[0]);
        await c2.AddBlankBlock(bBlocks[0]);

        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for SaveToDatabase test in ClinicalTest");
        Container ctContainer = await DatabaseService.Instance.Database.CreateContainerAsync("ClinicalTestRemoveFromDatabase", "/PartitionKey");
        Container blockContainer = await DatabaseService.Instance.Database.CreateContainerAsync("BlockClinicalTestRemoveFromDatabase", "/PartitionKey");


        // Act
        c2.SaveToDatabase(true);

        while (ProcessQueue.Instance.IsRunning[guidClinicalTest])
        {
            
        }

        await c2.RemoveFromDatabase();


        await Assert.ThrowsAnyAsync<CosmosException>(() => blockContainer.ReadItemAsync<BlockClinicalTestRemoveFromDatabase>(bBlocks[0].id, new PartitionKey(bBlocks[0].PartitionKey)));

        await Assert.ThrowsAnyAsync<CosmosException>(() => blockContainer.ReadItemAsync<BlockClinicalTestRemoveFromDatabase>(nBlocks[0].id, new PartitionKey(nBlocks[0].PartitionKey)));

        await Assert.ThrowsAnyAsync<CosmosException>(() => ctContainer.ReadItemAsync<ClinicalTestRemoveFromDatabase>(c2.id, new PartitionKey(c2.PartitionKey)));


        // Clean up
        await ctContainer.DeleteContainerAsync();
        await blockContainer.DeleteContainerAsync();
    }

}



public class BlockClinicalTestRemoveFromDatabase : Block { }
public class ClinicalTestRemoveFromDatabase : ClinicalTest { }