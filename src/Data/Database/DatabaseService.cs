﻿using System;
using Microsoft.Azure.Cosmos;
namespace src.Data;

public class DatabaseService 
{
    private static readonly DatabaseService instance = new DatabaseService();

    private CosmosClient client;
    public Database? Database;
    private DatabaseService()
    {
        client = new CosmosClient(Environment.GetEnvironmentVariable("COSMOS_API_ENDPOINT"), Environment.GetEnvironmentVariable("COSMOS_API_KEY"));
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