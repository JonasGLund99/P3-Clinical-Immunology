using Microsoft.Azure.Cosmos;

namespace src.Data;

public abstract class BaseModel<T> where T : BaseModel<T>
{
	public BaseModel(string id)
	{
		this.id = id;
	}
    public BaseModel() { }

    public string id { get; set; } = "";

    public async Task SaveToDatabase()
	{
		Database? db = DatabaseService.Instance.Database;

		if (db == null) throw new NullReferenceException("There was no reference to the database");

		await db.GetContainer(typeof(T).Name).UpsertItemAsync<T>(
			item: (T) this
        );
    }

	public async Task RemoveFromDatabase()
	{
        Database? db = DatabaseService.Instance.Database;

        if (db == null) throw new NullReferenceException("There was no reference to the database");

        await db.GetContainer(typeof(T).Name).DeleteItemAsync<T>(
            id: this.id, 
			partitionKey: new PartitionKey(this.id)
        );
    }
}
