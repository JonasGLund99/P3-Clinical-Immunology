using Microsoft.Azure.Cosmos;
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
                    nplicatesInBlock: 72,
                    experimentIds: new List<string>() { e.id },
                    slides: new List<Slide>(),
                    analyteNames: new List<string>()
                );
                await ct.SaveToDatabase();
            }
        }
    }
}


class Exp : BaseModel<Exp> {
    public Exp(string id, string experimentNumber, string title, string author, string description) : base(id)
    {
        ExperimentNumber = experimentNumber;
        Title = title;
        Author = author;
        Description = description;
    }
    public string ExperimentNumber { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
}

class CT : BaseModel<CT> {
    public CT(string id, string title, List<string> experimentIds) : base(id)
    {
        Title = title;
        ExperimentIds = experimentIds;
    }
    public string Title { get; set; }
    public List<string> ExperimentIds { get; set; }
}