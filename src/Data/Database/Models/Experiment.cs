using Microsoft.Azure.Cosmos;
using System.ComponentModel.DataAnnotations;

namespace src.Data;

public class Experiment : BaseModel<Experiment>
{
    public Experiment(string id, string experimentNumber, string title, string author, string description, DateTime createdAt) : base(id)
    {
        ExperimentNumber = experimentNumber;
        Title = title;
        Author = author;
        Description = description;
        CreatedAt = createdAt;
    }
    public Experiment(string id) : base(id) 
    {

    }
    public Experiment() : base() 
    {

    }
    public string ExperimentNumber { get; set; } = "";
    [Required]
    public string Title { get; set; } = "";
    [Required]
    public string Author { get; set; } = "";
    [Required]
    public string Description { get; set; } = "";
    public List<string> ClinicalTestIds { get; set; } = new List<string>();
    [Required]
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public DateTime EditedAt { get; private set; } = DateTime.Now;

    public async Task<List<ClinicalTest>> QueryClinicalTests(string searchParameter) {
        List<ClinicalTest> clinicalTests = new List<ClinicalTest>();
        if(DatabaseService.Instance.Database == null)
        {
            throw new NullReferenceException("No database");
        }
        string queryString = @"SELECT * FROM ClinicalTest
                            WHERE CONTAINS(ClinicalTest.Title, @searchParameter, true) 
                            AND ARRAY_CONTAINS(ClinicalTest.ExperimentIds, @expId)";

        FeedIterator<ClinicalTest> feed = DatabaseService.Instance.Database.GetContainer("ClinicalTest")
                                        .GetItemQueryIterator<ClinicalTest>(
                                            queryDefinition: new QueryDefinition(queryString)
                                            .WithParameter("@searchParameter", searchParameter)
                                            .WithParameter("@expId", this.id)
                                        );
        while (feed.HasMoreResults)
        {
            FeedResponse<ClinicalTest> response = await feed.ReadNextAsync();
            clinicalTests.AddRange(response);

        }
        return clinicalTests;
    }
}
