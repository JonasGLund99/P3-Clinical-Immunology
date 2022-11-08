using System;
using Microsoft.Azure.Cosmos;
namespace src.Data;

public class DatabaseService 
{
    private static readonly DatabaseService instance = new DatabaseService();

    private CosmosClient client;
    public Database? Database;
    private DatabaseService()
    {
        client = new CosmosClient("https://asbjoernjc.documents.azure.com:443/", "Ny5RJxrhuBR5gH4C3BFIGOBdq8BPpkNnHdqWqBZuU5pMcmkVWUzYA8lJxOpat73WRQU5IfKOq4qxACDbf06Gng==");
    }
    public static DatabaseService Instance
    {
        get
        {
            return instance;
        }
    }
    
    public async Task SetupDatabase()
    {
        Database = await client.CreateDatabaseIfNotExistsAsync("ClinicalImmunology",1000);
        await Database.CreateContainerIfNotExistsAsync("Experiment", "/id");
        await Database.CreateContainerIfNotExistsAsync("ClinicalTest", "/id");
        await Database.CreateContainerIfNotExistsAsync("Slide", "/id");
        await Database.CreateContainerIfNotExistsAsync("Block", "/id");
    }

}
