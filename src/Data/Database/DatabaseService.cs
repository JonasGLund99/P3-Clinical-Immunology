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
        client = new CosmosClient("https://p3-database.documents.azure.com:443/", "hB3Q6QxvgjERggOZC3vjRuRjzYjbpBJMXM6vKK2bboF3D4fBhDMmfsI0sHpk5qFIdzNSozPuGSoyF3cOzoeIjg==");
        SetupDatabase();
    }
    public static DatabaseService Instance
    {
        get
        {
            return instance;
        }
    }
    
    public async void SetupDatabase()
    {
        Database = await client.CreateDatabaseIfNotExistsAsync("ClinicalImmunology",1000);
        await Database.CreateContainerIfNotExistsAsync("Experiment", "/id");
        await Database.CreateContainerIfNotExistsAsync("ClinicalTest", "/id");
        await Database.CreateContainerIfNotExistsAsync("Slide", "/id");
        await Database.CreateContainerIfNotExistsAsync("Block", "/id");
    }

}
