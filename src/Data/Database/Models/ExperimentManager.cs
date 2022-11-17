using Microsoft.Azure.Cosmos;

namespace src.Data;

public static class ExperimentManager
{
    public static async Task<List<Experiment>> QueryExperiments(string searchParameter)
    {
        List<Experiment> experiments = new List<Experiment>();
        if (DatabaseService.Instance.Database == null)
        {
            throw new NullReferenceException("No database");
        }
        
        string queryString = @"SELECT * FROM Experiment 
                            WHERE CONTAINS(Experiment.ExperimentNumber, @searchParameter, true)
                            OR CONTAINS(Experiment.Title, @searchParameter, true)
                            OR CONTAINS(Experiment.Author, @searchParameter, true)";

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

    public static async Task<T> GetObjectById<T>(string searchContainer, string idParameter)
    {
        List<T> objects = new List<T>();

        if (DatabaseService.Instance.Database == null)
        {
            throw new NullReferenceException("No database");
        }
        string queryString = @"SELECT * FROM " + searchContainer + 
                            " WHERE " + searchContainer +".id = @expid";

        FeedIterator<T> feed = DatabaseService.Instance.Database.GetContainer(searchContainer)
                                        .GetItemQueryIterator<T>(
                                            queryDefinition: new QueryDefinition(queryString)
                                            .WithParameter("@expid", idParameter)
                                        );
        
        while (feed.HasMoreResults)
        {
            FeedResponse<T> response = await feed.ReadNextAsync();
            objects.AddRange(response);
        }
        if (objects.Count != 1)
            throw new ArgumentException("0 or more than 1 objects matched the id");

        return objects[0];
    }

    public static async Task<Experiment> GetExperimentById(string id)
    {
        Experiment e = await GetObjectById<Experiment>("Experiment", id);
        return e;
    }

    public static async Task<ClinicalTest> GetClinicalTestById(string id)
    {
        ClinicalTest ct = await GetObjectById<ClinicalTest>("ClinicalTest", id);
        return ct;
    }

    // Deletes the relation between an experiment and clinical test, 
    // and deletes the clinical test if it is not related to any other experiments
    public static async Task Disassociate(Experiment experiment, ClinicalTest clinicalTest)
    {
        experiment.ClinicalTestIds.Remove(clinicalTest.id);
        await experiment.SaveToDatabase();

        clinicalTest.ExperimentIds.Remove(experiment.id);
        
        if (clinicalTest.ExperimentIds.Count == 0)
        {
            await clinicalTest.RemoveFromDatabase();
        } 
        else 
        {
            await clinicalTest.SaveToDatabase();
        }
    }
    
   public static async Task Associate(Experiment experiment, ClinicalTest clinicalTest)
   {
      if ( !experiment.ClinicalTestIds.Contains(clinicalTest.id))
      {
         experiment.ClinicalTestIds.Add(clinicalTest.id);
         await experiment.SaveToDatabase();

         clinicalTest.ExperimentIds.Add(experiment.id);
         await clinicalTest.SaveToDatabase();
      }
   }

    public static async Task DeleteExperiment(Experiment experiment)
    {
        List<ClinicalTest> clinicalTests = await experiment.QueryClinicalTests(""); 
        foreach (ClinicalTest clinicalTest in clinicalTests)
        {
            await Disassociate(experiment, clinicalTest);
        }
        await experiment.RemoveFromDatabase();
    }

   public static async Task DeleteClinicalTest(ClinicalTest clinicalTest)
   {
        List<string> ids = new();
        ids.AddRange(clinicalTest.ExperimentIds); 
        foreach (string id in ids)
        {
            Experiment e = await GetExperimentById(id);
            await Disassociate(e, clinicalTest);
        }
   }

}
