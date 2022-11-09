namespace src.Data;

class Experiment : BaseModel<Experiment>
{
    public Experiment(string experimentNumber, string title, string author, string description, string id) : base(id)
    {
        ExperimentNumber = experimentNumber;
        Title = title;
        Author = author;
        Description = description;
        ClinicalTests = new List<ClinicalTest>();
        CreatedAt = DateTime.Now;
        EditedAt = CreatedAt;
    }

    public string ExperimentNumber { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public List<ClinicalTest> ClinicalTests { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime EditedAt { get; private set; }

}