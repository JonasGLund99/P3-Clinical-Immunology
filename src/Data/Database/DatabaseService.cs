using System;
using System.Linq.Expressions;
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
        Database = await client.CreateDatabaseIfNotExistsAsync("ClinicalImmunology", 1000);
        await Database.CreateContainerIfNotExistsAsync("Experiment", "/id");
        await Database.CreateContainerIfNotExistsAsync("ClinicalTest", "/id");
    }

    public async Task<T?> GetItemById<T>(string id)
    {
        if (Database == null)
            throw new NullReferenceException("Database is null");

        T? res;
        try
        {
            res = await Database.GetContainer(typeof(T).Name).ReadItemAsync<T>(id, new PartitionKey(id));
        }
        catch
        {
      
            res = default;
        }
        return res;
    } 
}
