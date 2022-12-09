using Xunit;
using Microsoft.Azure.Cosmos;
using src.Data;

namespace src.Tests;

public class TestItem : BaseModel<TestItem>
{
    public TestItem(string id) : base(id)
    {
        PartitionKey = id;
    }

    public override string PartitionKey { get; set; }
}