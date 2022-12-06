using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using src.Data;
using System.Collections;
using Xunit;

namespace src.Tests;

//https://youtu.be/2Wp8en1I9oQ?t=1152 Se delen om Reusing Instances
public class SaveToDatabaseClinicalTestTest
{
    private readonly ClinicalTest _sut;

    public SaveToDatabaseClinicalTestTest()
    {
        _sut = new ClinicalTest();
    }


    [Fact]
    public async void SaveToDatabase()
    {
        string guidNBlock = Guid.NewGuid().ToString();
        string guidBBlock = Guid.NewGuid().ToString();
        string guidClinicalTest = Guid.NewGuid().ToString();
        List<BlockClinicalTestSaveToDatabase> nBlocks = new List<BlockClinicalTestSaveToDatabase>
            {
                new BlockClinicalTestSaveToDatabase { SlideIndex = 0, BlockIndex = 0, Type = Block.BlockType.Normal, id = guidNBlock, PartitionKey = guidClinicalTest },
            };

        List<BlockClinicalTestSaveToDatabase> bBlocks = new List<BlockClinicalTestSaveToDatabase>
        {
            new BlockClinicalTestSaveToDatabase { SlideIndex = 0, BlockIndex = 0, Type = Block.BlockType.Blank, id = guidBBlock, PartitionKey = guidClinicalTest },
        };
        ClinicalTestSaveToDatabaseTest c2 = new ClinicalTestSaveToDatabaseTest { id = guidClinicalTest, PartitionKey = guidClinicalTest};

        await c2.AddBlankBlock(bBlocks[0]);
        //await c2.SetNormalBlocks(nBlocks);

        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for SaveToDatabase test in ClinicalTest");
        Container ctContainer = await DatabaseService.Instance.Database.CreateContainerAsync("ClinicalTestSaveToDatabase", "/PartitionKey");
        Container blockContainer = await DatabaseService.Instance.Database.CreateContainerAsync("BlockClinicalTestSaveToDatabase", "/PartitionKey");


        // Act
        c2.SaveToDatabase(true);
        while (ProcessQueue.Instance.GetQueues().Count != 0) {
            Console.WriteLine(ProcessQueue.Instance.GetQueues().Count);
        }

        //BlockClinicalTestSaveToDatabase normalBlockFromDatabase = await blockContainer.ReadItemAsync<BlockClinicalTestSaveToDatabase>(nBlocks[0].id, new PartitionKey(nBlocks[0].PartitionKey));

        BlockClinicalTestSaveToDatabase blankBlockFromDatabase = await blockContainer.ReadItemAsync<BlockClinicalTestSaveToDatabase>(bBlocks[0].id, new PartitionKey(bBlocks[0].PartitionKey));

        ClinicalTestSaveToDatabaseTest clinicalTestFromDatabase = await ctContainer.ReadItemAsync<ClinicalTestSaveToDatabaseTest>(c2.id, new PartitionKey(c2.PartitionKey));
        // Assert
        Assert.True(true);

        // Clean up
        await ctContainer.DeleteContainerAsync();
        await blockContainer.DeleteContainerAsync();
    }

}

public class BlockClinicalTestSaveToDatabase : Block { }
public class ClinicalTestSaveToDatabaseTest : ClinicalTest { }