namespace src.Data;

class SlideDataFile
{
    public SlideDataFile(string filename, string content)
    {
        Filename = filename;
        Content = content;
    }
    public string Filename { get; set; }
    public string Content { get; set; }
}