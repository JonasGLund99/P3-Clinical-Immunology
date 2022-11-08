using Microsoft.Azure.Cosmos;
using System;

namespace src.Data;

public abstract class BaseModel
{
	public BaseModel(string id)
	{
		this.id = id;
	}

    public string id;

    public async Task GenericSaveToDatabase<T>() where T: BaseModel
	{
		await DatabaseService.Instance.SetupDatabase(); /* Waits for setupdatabase to finish so the database is running reference*/
		Database? db = DatabaseService.Instance.Database;

		if (db == null) throw new NullReferenceException("There was no reference to the database"); /* If the database somehow failed, we throw an error */

		await db.GetContainer(typeof(T).Name).UpsertItemAsync<T>(
			item: (T) this
        );
    }

	public async Task GenericRemoveFromDatabase<T>() where T: BaseModel
	{
        await DatabaseService.Instance.SetupDatabase();
        Database? db = DatabaseService.Instance.Database;

        if (db == null) throw new NullReferenceException("There was no reference to the database");

        await db.GetContainer(typeof(T).Name).DeleteItemAsync<T>(
            id: this.id, 
			partitionKey: new PartitionKey(this.id)
        );
    }
}
