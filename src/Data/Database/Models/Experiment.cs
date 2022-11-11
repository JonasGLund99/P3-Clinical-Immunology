using Microsoft.Azure.Cosmos;

namespace src.Data;

public class Experiment : BaseModel<Experiment>
{
    public Experiment(string id, string experimentNumber, string title, string author, string description, List<string> clinicalTestIds, DateTime createdAt, DateTime editedAt) : base(id)
    {
        ExperimentNumber = experimentNumber;
        Title = title;
        Author = author;
        Description = description;
        ClinicalTestIds = clinicalTestIds;
        CreatedAt = createdAt;
        EditedAt = editedAt;
    }
    public Experiment(string id) : base(id) { }

    public string ExperimentNumber { get; set; } = "";
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> ClinicalTestIds { get; set; } = new List<string>();
    public List<ClinicalTest> ClinicalTests = new List<ClinicalTest>();
    public DateTime CreatedAt { get; private set; } = DateTime.Now;
    public DateTime EditedAt { get; private set; } = DateTime.Now;

    public async Task<List<ClinicalTest>> QueryClinicalTests(string searchParameter) {
        List<ClinicalTest> clinicalTests = new List<ClinicalTest>();
        if(DatabaseService.Instance.Database == null)
        {
            throw new NullReferenceException("No database");
        }
        string queryString = @"SELECT * FROM ClinicalTest
                            WHERE Title LIKE '%@searchParameter%'";

        FeedIterator<ClinicalTest> feed = DatabaseService.Instance.Database.GetContainer("ClinicalTest")
                                        .GetItemQueryIterator<ClinicalTest>(
                                            queryDefinition: new QueryDefinition(queryString)
                                            .WithParameter("@searchParameter", searchParameter)
                                        );
        while (feed.HasMoreResults)
        {
            FeedResponse<ClinicalTest> response = await feed.ReadNextAsync();
            clinicalTests.AddRange(response);

        }
        return clinicalTests;
    }
}