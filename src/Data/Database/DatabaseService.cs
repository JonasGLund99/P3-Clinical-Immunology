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
            Client = new CosmosClient("https://johannes-account.documents.azure.com:443/", "4a2gm67DTBBbiUqwZFl3yp3CXjYjAvsIxSLwP4sRUWb8oZSs38HpUF9v6PMjR4avRg4ClBkY9Bc6ACDbXHhKNQ==");
        }
        else
        {
            Client = new CosmosClient("https://johannes-account.documents.azure.com:443/", "4a2gm67DTBBbiUqwZFl3yp3CXjYjAvsIxSLwP4sRUWb8oZSs38HpUF9v6PMjR4avRg4ClBkY9Bc6ACDbXHhKNQ==");
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
