namespace src.Data;

class Experiment : BaseModel
{
    public Experiment(string id, string experimentNumber, string title, string author, string description) : base(id)
    {
        ExperimentNumber = experimentNumber;
        Title = title;
        Author = author;
        Description = description;
        CreatedAt = DateTime.Now;
        EditedAt = CreatedAt;
    }

    public string ExperimentNumber { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public List<ClinicalTest> ClinicalTests = new List<ClinicalTest>();
    public DateTime CreatedAt { get; private set; }
    public DateTime EditedAt { get; private set; }

    public void UpdateEditedAt(DateTime editedAt)
    {
        EditedAt = editedAt;
    }

}