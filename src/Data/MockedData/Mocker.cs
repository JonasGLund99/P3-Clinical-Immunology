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
            Exp e = new Exp(
                id: Guid.NewGuid().ToString(),
                experimentNumber: $"EXP-{i + 1}",
                title: $"Experiment {i + 1}",
                author: authors[r.Next(0, 5)],
                description: "Some description"
            );
            await e.SaveToDatabase();

            int numCT = r.Next(1, 6);
            for (int j = 0; j < numCT; j++) {
                CT ct = new CT(
                    id: Guid.NewGuid().ToString(),
                    title: $"Test {j + 1} in {e.ExperimentNumber}",
                    experimentIds: new List<string>() { e.id }
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