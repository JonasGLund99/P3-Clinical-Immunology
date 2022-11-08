using Microsoft.Azure.Cosmos;
using System;
using System.ComponentModel;
using System.Security.Policy;
using System.Threading.Tasks;

namespace src.Data;

public abstract class BaseModel
{

	/*Creates a new instance of the CosmosClient class with the database URI and key*/
	public string id;
	private string partitionValue = "id";

	public BaseModel(string id)
	{
		this.id = id;
	}

	/* Removes a given item from the correct container in the database /
    /public async Task removeDataBaseItem()
    {
        const item = container.item('28a31558-ff8c-40c3-a7e8-1e8904c5ff72', '<partition-key-value')
    }/

    / Adds a given item to the correct container in the database */
	public async Task addDataBaseItem<T>(T modelObject) where T : BaseModel
	{
		CosmosClient client = new(
			accountEndpoint: "https://asbjoernjc.documents.azure.com:443/",
			authKeyOrResourceToken: "Ny5RJxrhuBR5gH4C3BFIGOBdq8BPpkNnHdqWqBZuU5pMcmkVWUzYA8lJxOpat73WRQU5IfKOq4qxACDbf06Gng=="
		);

		Database database = await client.CreateDatabaseIfNotExistsAsync(
			id: "P3Test"
		);

		//Create new object and upsert (create or replace) to container
		Container container = await database.CreateContainerIfNotExistsAsync(
			id: "nplicate",
			partitionKeyPath: $"/{partitionValue}",
			throughput: 400
		);

		T createdItem = await container.UpsertItemAsync<T>(
			item: modelObject,
			partitionKey: new PartitionKey(modelObject.partitionValue)
		);

		Console.WriteLine($"Created item:\t{createdItem.id}");
	}


}

public class Nplicate : BaseModel
{
	public Nplicate(string id) : base(id)
	{
	}
};