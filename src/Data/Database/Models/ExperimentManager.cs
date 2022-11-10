using Microsoft.Azure.Cosmos;

namespace src.Data;

static class ExperimentManager
{
    public static async Task<List<Experiment>> QueryExperiments(string searchParameter)
    {
        List<Experiment> experiments = new List<Experiment>();
        if(DatabaseService.Instance.Database == null)
        {
            throw new NullReferenceException("No database");
        }
        string queryString = @"SELECT * FROM Experiment 
                            WHERE ExperimentNumber LIKE '%@searchParameter%'
                            OR Title LIKE '%@searchParameter%'
                            OR Author LIKE '%@searchParameter%'";
        FeedIterator<Experiment> feed = DatabaseService.Instance.Database.GetContainer("Experiment")
                                        .GetItemQueryIterator<Experiment>(
                                            queryDefinition: new QueryDefinition(queryString)
                                            .WithParameter("@searchParameter", searchParameter)
                                        );
        while (feed.HasMoreResults)
        {
            FeedResponse<Experiment> response = await feed.ReadNextAsync();
            experiments.AddRange(response);

        }
        return experiments;
    }

    public static void Disassociate(Experiment experiment, ClinicalTest clinicalTest)
    {
        experiment.ClinicalTests.Remove(clinicalTest);
        clinicalTest.Experiments.Remove(experiment);
        if (clinicalTest.Experiments.Count == 0)
        {
            clinicalTest.Delete();
        }
    }
    
    public static void Associate(Experiment experiment, ClinicalTest clinicalTest)
    {
        experiment.ClinicalTests.Add(clinicalTest);
        clinicalTest.Experiments.Add(experiment);
    }

    public static void DeleteExperiment(Experiment experiment)
    {
        foreach (ClinicalTest clinicalTest in experiment.ClinicalTests)
        {
            Disassociate(experiment, clinicalTest);
        }
        experiment.RemoveFromDatabase();
    }
}