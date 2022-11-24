using Microsoft.Azure.Cosmos;
using System.IO;
using src.Data;

static class Mocker
{
    static int numExperiments = 1;
    static int numSlides = 16;

    static List<string> authors = new List<string>() {
        "Rikke Bæk",
        "Malene Hansen",
        "Knud Eriksen",
        "Hanne Nilsen",
        "Ritta Knudsen"
    };

    static List<SlideDataFile> slideDataFiles = new List<SlideDataFile> {
        new SlideDataFile(
            filename: "10000465_0016_flag.txt",
            content: File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data/MockedData/10000465_0016_flag.txt"))
        ),
        new SlideDataFile(
            filename: "10000466_0014_flag.txt",
            content: File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data/MockedData/10000466_0014_flag.txt"))
        ),
        new SlideDataFile(
            filename: "10000467_0005_flag.txt",
            content: File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data/MockedData/10000467_0005_flag.txt"))
        )
    };

    static Random r = new Random();

    public static async Task Mock(Database database)
    {
        for (int i = 0; i < numExperiments; i++)
        {
            Experiment e = new Experiment(
                id: Guid.NewGuid().ToString(),
                experimentNumber: $"EXP-{i + 1}",
                title: $"Experiment 16 slides",
                author: authors[r.Next(0, 5)],
                description: "Some description",
                createdAt: DateTime.Now
            );
            await e.SaveToDatabase();

            int numCT = r.Next(1, 3);
            // int numCT = 1;
            for (int j = 0; j < numCT; j++)
            {
                ClinicalTest ct = new ClinicalTest(
                    id: Guid.NewGuid().ToString(),
                    title: $"Test {j + 1} in {e.ExperimentNumber}",
                    nplicateSize: 3,
                    description: "Some description",
                    createdAt: DateTime.Now
                );
                ct.SlideDataFiles = slideDataFiles;
                for (int l = 0; l < numSlides; l++)
                {
                    ct.AddSlide(
                    slide: new Slide(Guid.NewGuid().ToString(), (10000465+l).ToString()),
                    patientData: new List<string>[21].Select(l => new List<string>()).ToArray()
                );
                }
                ct.TableTitles = new List<string>() { "key1", "key2", "key3", "key4", "key5" };
                ct.ChosenTableTitles = new string[] { "key2", "key1", "key3" };
                //ct.CalculateClinicalTestResult();
                await ct.SaveToDatabase();
                await ExperimentManager.Associate(e, ct);
            }
        }
    }
}
