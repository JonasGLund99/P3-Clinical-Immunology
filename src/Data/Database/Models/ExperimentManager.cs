namespace src.Data;

class ExperimentManager
{
    public static List<Experiment> Experiments = new List<Experiment>();

    static public List<Experiment> QueryExperiments(string searchParameter)
    {
        return Experiments.FindAll(exp =>
        {
            if (exp.ExperimentNumber.Contains(searchParameter) ||
                exp.Title.Contains(searchParameter) ||
                exp.Author.Contains(searchParameter))
            {
                return true;
            }
            return false;
        });
    }

    static public void Disassociate(Experiment experiment, ClinicalTest clinicalTest)
    {
        experiment.ClinicalTests.Remove(clinicalTest);
        clinicalTest.Experiments.Remove(experiment);
        if (clinicalTest.Experiments.Count == 0)
        {
            clinicalTest.Delete();
        }
    }
    
    static public void Associate(Experiment experiment, ClinicalTest clinicalTest)
    {
        experiment.ClinicalTests.Add(clinicalTest);
        clinicalTest.Experiments.Add(experiment);
    }

    static public void DeleteExperiment(Experiment experiment)
    {
        Experiments.Remove(experiment);
        foreach (ClinicalTest clinicalTest in experiment.ClinicalTests)
        {
            Disassociate(experiment, clinicalTest);
        }
        experiment.RemoveFromDatabase();
    }
}