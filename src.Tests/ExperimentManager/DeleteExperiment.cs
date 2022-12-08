//using Xunit;
//using Microsoft.Azure.Cosmos;
//using src.Data;
//using System.Collections;

//namespace src.Tests;

//public class DeleteExperimentTest
//{
//    [Theory]
//    [ClassData(typeof(SearchByBarcodeOrTitleData))]
//    public async void DeleteExperiment(bool expected, params Experiment[] experiments)
//    {
//        DatabaseService.EnableTestMode();
//        await DatabaseService.Instance.SetupDatabase();
//        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for QueryExperiemnts test in Experiment Manager");

//        Experiment experiment1 = mockExperiment();
//        experiment1.Title = "Title";
//        experiment1.Author = "Jørn Christas";
//        experiment1.ExperimentNumber = "JE1903";
//        experiment1.SaveToDatabase();

//        Experiment experiment2 = mockExperiment();
//        experiment2.Title = "TestTitle";
//        experiment2.Author = "René";
//        experiment2.ExperimentNumber = "JE1902";
//        experiment2.SaveToDatabase();

//        Experiment experiment3 = mockExperiment();
//        experiment3.Title = "anothertesttitle";
//        experiment3.Author = "Papa Noël";
//        experiment3.ExperimentNumber = "FA7246";
//        experiment3.SaveToDatabase();

//        Experiment experiment4 = mockExperiment();
//        experiment4.Title = "IHaveSpecialCharacters æblemost";
//        experiment4.Author = "Jørn";
//        experiment4.ExperimentNumber = "GE7247";
//        experiment4.SaveToDatabase();

//        Experiment experiment5 = mockExperiment();
//        experiment5.Title = "gsjdfs";
//        experiment5.Author = "farooq";
//        experiment5.ExperimentNumber = "KL6432";
//        experiment5.SaveToDatabase();

//        while (ProcessQueue.Instance.IsRunning[experiment1.id] || ProcessQueue.Instance.IsRunning[experiment2.id] || ProcessQueue.Instance.IsRunning[experiment3.id] || ProcessQueue.Instance.IsRunning[experiment4.id])
//        {

//        }
        

//    }
//    class SearchByBarcodeOrTitleData : IEnumerable<object[]>
//    {
//        public IEnumerator<object[]> GetEnumerator()
//        {
//            // Cases for querying after Experiment.Title
//            yield return new object[] { "Title", 3 };
//            yield return new object[] { "TestTitle", 2 };
//            yield return new object[] { "anothertesttitle", 1 };
//            yield return new object[] { "IHaveSpecialCharacters æblemost", 1 };
//            yield return new object[] { "not existing", 0 };

//            // Cases for querying after Experiment.Author
//            yield return new object[] { "é", 1 };
//            yield return new object[] { "Jørn", 2 };
//            yield return new object[] { "Jørn Christas", 1 };
//            yield return new object[] { "Papa Noël", 1 };
//            yield return new object[] { "Christina Stürmer", 0 };

//            // Cases for querying after Experiment.ExperimentNumber
//            yield return new object[] { "JE190", 2 };
//            yield return new object[] { "FA7246", 1 };
//            yield return new object[] { "FA", 2 };
//            yield return new object[] { "ge7247", 1 };
//            yield return new object[] { "je99999999", 0 };
//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }
//    }

//    private Experiment mockExperiment()
//    {
//        Experiment experiment = new Experiment { id = Guid.NewGuid().ToString(), PartitionKey = Guid.NewGuid().ToString() };
//        List<ClinicalTest> clinicalTests = new List<ClinicalTest>
//        {
//            new ClinicalTest(Guid.NewGuid().ToString()),
//            new ClinicalTest(Guid.NewGuid().ToString()),
//            new ClinicalTest(Guid.NewGuid().ToString()),
//        };

//        return experiment;
//    }
//}