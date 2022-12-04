//using Microsoft.Azure.Cosmos;
//using src.Data;
//using System.Collections;
//using Xunit;
//namespace src.Tests;

////https://youtu.be/2Wp8en1I9oQ?t=1152 Se delen om Reusing Instances
//public class GetNormalBlocksTest
//{
//    private readonly Nplicate _sut;
//    private CosmosClient client;
//    private Database? Database;

//    public GetNormalBlocksTest()
//    {
//        _sut = new Nplicate();
//        client = new CosmosClient(Environment.GetEnvironmentVariable("COSMOS_API_ENDPOINT"), Environment.GetEnvironmentVariable("COSMOS_API_KEY"));
//    }

//    private async Task SetupTestDatabase()
//    {
//        Database = await client.CreateDatabaseIfNotExistsAsync("ClinicalImmunology4", 500);
//        await Database.CreateContainerIfNotExistsAsync("Experiment", "/PartitionKey");
//        await Database.CreateContainerIfNotExistsAsync("ClinicalTest", "/PartitionKey");
//        await Database.CreateContainerIfNotExistsAsync("Block", "/PartitionKey");
//    }


//    [Theory]
//    [ClassData(typeof(GetNormalBlocksTestData))]
//    public async void GetNormalBlocks(List<Block> expected, List<Block> normalBlocks, params ClinicalTest[] clinicalTests)
//    {
//        await SetupTestDatabase();

//        foreach (ClinicalTest clinicalTest in clinicalTests)
//        {
//            if (clinicalTest.normalBlocks == null)
//            {
//                List<Block> result = new List<Block>();

//                foreach (string id in NormalBlockIds)
//                {
//                    Block? b = await DatabaseService.Instance.GetItemById<Block>(id, this.PartitionKey);
//                    if (b == null) throw new NullReferenceException("Block is null");
//                    result.Add(b);
//                }
//                normalBlocks = result;
//            }
//        }

//        Assert.Equal(expected, );
//    }

//    public class GetNormalBlocksTestData : IEnumerable<object[]>
//    {
//        public IEnumerator<object[]> GetEnumerator()
//        {
//            Nplicate n1 = new Nplicate("PP13");
//            n1.Spots.Add(new Spot(22278, true));
//            n1.Spots.Add(new Spot(19855, false));
//            n1.Spots.Add(new Spot(24627, false));


//            Nplicate n2 = new Nplicate("c69");
//            n2.Spots.Add(new Spot(0, true));
//            n2.Spots.Add(new Spot(0, true));
//            n2.Spots.Add(new Spot(0, true));


//            yield return new object[] { 22241, n1 };
//            yield return new object[] { 0, n2 };

//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }
//    }

//}
