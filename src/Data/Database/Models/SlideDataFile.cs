namespace src.Data;

public class SlideDataFile
{
    public SlideDataFile(string filename, string content)
    {
        Filename = filename;
        Content = content;
    }
    public SlideDataFile() { }
    public string Filename { get; set; } = "";
    public string Content { get; set; } = "";
}