using Microsoft.Azure.Cosmos;
using src.Data;

static class Mocker
{
    static int numExperiments = 20;
    static List<string> authors = new List<string>() {
        "Rikke Bæk",
        "Malene Hansen",
        "Knud Eriksen",
        "Hanne Nilsen",
        "Ritta Knudsen"
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
                clinicalTestIds: new List<string>(),
                createdAt: DateTime.Now,
                editedAt: DateTime.Now
            );
            await e.SaveToDatabase();

            int numCT = r.Next(1, 6);
            for (int j = 0; j < numCT; j++) {
                ClinicalTest ct = new ClinicalTest(
                    id: Guid.NewGuid().ToString(),
                    title: $"Test {j + 1} in {e.ExperimentNumber}",
                    nplicateSize: 3,
                    description: "Some description",
                    createdAt: DateTime.Now,
                    editedAt: DateTime.Now,
                    slideDataFiles: new List<SlideDataFile>(),
                    patientKeys: new Dictionary<string, bool>(),
                    activeKeys: new List<string>(),
                    experimentIds: new List<string>() { e.id },
                    slides: new List<Slide>(),
                    analyteNames: new List<string>()
                );
                await ct.SaveToDatabase();
            }
        }
    }
}
