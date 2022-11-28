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
    public abstract string PartitionKey { get; set; }

    public void SaveToDatabase()
    {
        Database? db = DatabaseService.Instance.Database;
		if (db == null) throw new NullReferenceException("There was no reference to the database");

        var process = () => {
            var result = db.GetContainer(typeof(T).Name).UpsertItemAsync<T>(
                item: (T) this,
                partitionKey: new PartitionKey(this.PartitionKey)
            );
            System.Console.WriteLine("Saved: " + typeof(T).Name);
            return result;
        };

        if (typeof(T) == typeof(Block))
        {
            ProcessQueue.Instance.Enqueue(process, this.PartitionKey);
        }
        else
        {
            ProcessQueue.Instance.Enqueue(process, this.id);
        }
    }
    public async Task saveToDatabaseAsync()
    {
        Database? db = DatabaseService.Instance.Database;
		if (db == null) throw new NullReferenceException("There was no reference to the database");
        
        await db.GetContainer(typeof(T).Name).UpsertItemAsync<T>(
            item: (T) this,
            partitionKey: new PartitionKey(this.PartitionKey)
        );
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
