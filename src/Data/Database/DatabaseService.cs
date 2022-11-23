using System;
using Microsoft.Azure.Cosmos;
namespace src.Data;

public class DatabaseService 
{
    private static readonly DatabaseService instance = new DatabaseService();

    private CosmosClient client;
    private DatabaseService()
    {
        client = new CosmosClient(Environment.GetEnvironmentVariable("COSMOS_API_ENDPOINT"), Environment.GetEnvironmentVariable("COSMOS_API_KEY"));
    }
    public static DatabaseService Instance
    {
        get
        {
            return instance;
        }
    }
    public Database? Database;

    
    public async Task SetupDatabase()
    {
        Database = await client.CreateDatabaseIfNotExistsAsync("ClinicalImmunology2", 500);
        await Database.CreateContainerIfNotExistsAsync("Experiment", "/PartitionKey");
        await Database.CreateContainerIfNotExistsAsync("ClinicalTest", "/PartitionKey");
        await Database.CreateContainerIfNotExistsAsync("Block", "/PartitionKey");
    }

    public async Task<T> GetItemById<T>(string id, string partitionKey)
    {
        if (Database == null)
            throw new NullReferenceException("Database is null");

        return await Database.GetContainer(typeof(T).Name).ReadItemAsync<T>(id, new PartitionKey(partitionKey));
    }
}
