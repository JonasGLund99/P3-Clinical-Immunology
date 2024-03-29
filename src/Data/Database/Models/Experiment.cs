﻿using Microsoft.Azure.Cosmos;
using System.ComponentModel.DataAnnotations;

namespace src.Data;

public class Experiment : BaseModel<Experiment>
{
    public Experiment(string id, string experimentNumber, string title, string author, string description, DateTime createdAt) : base(id)
    {
        PartitionKey = id;
        ExperimentNumber = experimentNumber;
        Title = title;
        Author = author;
        Description = description;
        CreatedAt = createdAt;
    }
    public Experiment(string id) : base(id) 
    {
        PartitionKey = id;
    }
    public Experiment() : base() { }

    public override string PartitionKey { get; set; } = "";
    public string ExperimentNumber { get; set; } = "";
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> ClinicalTestIds { get; set; } = new List<string>();
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public DateTime EditedAt { get; set; } = DateTime.Now;

    public override async Task SaveToDatabaseAsync()
    {
        EditedAt = DateTime.Now;
        await base.SaveToDatabaseAsync();
    }

    public async Task<List<ClinicalTest>> QueryClinicalTests(string searchParameter) {
        List<ClinicalTest> clinicalTests = new List<ClinicalTest>();
        if(DatabaseService.Instance.Database == null)
        {
            throw new NullReferenceException("No database");
        }

        string queryString = @"SELECT VALUE CT
                                FROM ClinicalTest CT
                                WHERE 
                                (
                                    EXISTS (
                                        SELECT VALUE s
                                        FROM s IN CT.Slides
                                        WHERE CONTAINS(s.Barcode, @searchParameter, true)
                                        )
                                    OR CONTAINS(CT.Title, @searchParameter, true)
                                )
                                AND ARRAY_CONTAINS(CT.ExperimentIds, @expId)
                                ORDER BY CT.EditedAt DESC ";

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
