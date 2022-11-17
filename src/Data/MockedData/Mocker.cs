using Microsoft.Azure.Cosmos;
using System.IO;
using src.Data;

static class Mocker
{
    static int numExperiments = 5;

    static List<string> authors = new List<string>() {
        "Rikke Bæk",
        "Malene Hansen",
        "Knud Eriksen",
        "Hanne Nilsen",
        "Ritta Knudsen"
    };

    static List<SlideDataFile> slideDataFiles = new List<SlideDataFile> {
        new SlideDataFile(
            filename: "file1",
            content: File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data/MockedData/10000465_0016_flag.txt"))
        ),
        new SlideDataFile(
            filename: "file2",
            content: File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data/MockedData/10000466_0014_flag.txt"))
        ),
        new SlideDataFile(
            filename: "file3",
            content: File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data/MockedData/10000467_0005_flag.txt"))
        )
    };

    static Random r = new Random();

    public static async Task Mock(Database database) {
        for (int i = 0; i < numExperiments; i++) {
            Experiment e = new Experiment(
                id: Guid.NewGuid().ToString(),
                experimentNumber: $"EXP-{i + 1}",
                title: $"Experiment {i + 1}",
                author: authors[r.Next(0, 5)],
                description: "Some description",
                createdAt: DateTime.Now
            );
            await e.SaveToDatabase();

            // int numCT = r.Next(1, 6);
            int numCT = 1;
            for (int j = 0; j < numCT; j++) {
                ClinicalTest ct = new ClinicalTest(
                    id: Guid.NewGuid().ToString(),
                    title: $"Test {j + 1} in {e.ExperimentNumber}",
                    nplicateSize: 3,
                    description: "Some description",
                    createdAt: DateTime.Now
                );
                ct.SlideDataFiles = slideDataFiles;
                ct.AddSlide(
                    slide: new Slide("10000465"),
                    patientData: new string[21][]
                );
                ct.AddSlide(
                    slide: new Slide("10000466"),
                    patientData: new string[21][]
                );
                ct.AddSlide(
                    slide: new Slide("10000467"),
                    patientData: new string[21][]
                );
                ct.TableTitles = new List<string>() { "key1", "key2", "key3", "key4", "key5" };
                ct.ChosenTableTitles = new string[] { "key2", "key1", "key3" };
                ct.CalculateClinicalTestResult();
                await ct.SaveToDatabase();
                await ExperimentManager.Associate(e, ct);
            }
        }
    }
}
