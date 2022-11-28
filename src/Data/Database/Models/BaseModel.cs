﻿using Microsoft.Azure.Cosmos;

namespace src.Data;

public abstract class BaseModel<T> where T : BaseModel<T>
{
	public BaseModel(string id)
	{
		this.id = id;
	}
    public BaseModel() { }

    public string id { get; set; } = "";
    public abstract string PartitionKey { get; set; }

    public void SaveToDatabase()
    {
        Database? db = DatabaseService.Instance.Database;
		if (db == null) throw new NullReferenceException("There was no reference to the database");

        var process = () => SaveToDatabaseAsync();

        if (typeof(T) == typeof(Block))
        {
            ProcessQueue.Instance.Enqueue(process, this.PartitionKey);
        }
        else
        {
            ProcessQueue.Instance.Enqueue(process, this.id);
        }
    }
    public virtual async Task SaveToDatabaseAsync()
    {
        const int MaxRetryCount = 3;
        int retryCount = 0;
        bool success = false;

        while (!success && retryCount < MaxRetryCount)
        {
            try 
            {
                Database? db = DatabaseService.Instance.Database;
                if (db == null) throw new NullReferenceException("There was no reference to the database");
                
                await db.GetContainer(typeof(T).Name).UpsertItemAsync<T>(
                    item: (T) this,
                    partitionKey: new PartitionKey(this.PartitionKey)
                );

                success = true;
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Failed to save " + typeof(T).Name + ". Retrying...");
                retryCount++;
            }
            
        }
        
    }

	public virtual async Task RemoveFromDatabase()
	{
        Database? db = DatabaseService.Instance.Database;

        if (db == null) throw new NullReferenceException("There was no reference to the database");

        await db.GetContainer(typeof(T).Name).DeleteItemAsync<T>(
            id: this.id, 
			partitionKey: new PartitionKey(this.PartitionKey)
        );
    }
}
