using System;
using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;
namespace src.Data;

public class DatabaseService 
{
    private static DatabaseService? instance;
    private static bool inTestMode = false;

    public CosmosClient Client;
    private DatabaseService()
    {
        if (inTestMode)
        {
            Client = new CosmosClient("https://asbjoernjc.documents.azure.com:443/", "Ny5RJxrhuBR5gH4C3BFIGOBdq8BPpkNnHdqWqBZuU5pMcmkVWUzYA8lJxOpat73WRQU5IfKOq4qxACDbf06Gng==");
        }
        else
        {
            Client = new CosmosClient("https://p3-database.documents.azure.com:443/", "nMTHGd46oUJ9xzWTS4kCmKhjbfqM5Qm7tbI1pt7knTvKpqBd48azK9cu1ZWoGKZZKiQgeItFWvxFACDbCLyEZw==");
        }
    }
    public static DatabaseService Instance
    {
        get
        {
            if (instance == null) 
            {
                instance = new DatabaseService();
            }
            return instance;
        }
    }
    
    public Database? Database;

    public static void EnableTestMode()
    {
        inTestMode = true;
    }
    public async Task SetupDatabase()
    {
        Database = await Client.CreateDatabaseIfNotExistsAsync("ClinicalImmunology2", 1000);
        await Database.CreateContainerIfNotExistsAsync("Experiment", "/PartitionKey");
        await Database.CreateContainerIfNotExistsAsync("ClinicalTest", "/PartitionKey");
        await Database.CreateContainerIfNotExistsAsync("Block", "/PartitionKey");
    }

    public async Task<T?> GetItemById<T>(string id, string partitionKey)
    {
        if (Database == null)
            throw new NullReferenceException("Database is null");

        T? res;
        try
        {
            res = await Database.GetContainer(typeof(T).Name).ReadItemAsync<T>(id, new PartitionKey(partitionKey));
        }
        catch
        {
            res = default;
        }
        return res;
    } 
}
