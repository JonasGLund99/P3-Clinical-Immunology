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

    public static async Task<Experiment> GetExperimentById(string id)
    {
        Experiment e = await DatabaseService.Instance.GetItemById<Experiment>(id);
        return e;
    }

    public static async Task<ClinicalTest> GetClinicalTestById(string id)
    {
        ClinicalTest ct = await DatabaseService.Instance.GetItemById<ClinicalTest>(id);
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
