using System.Text.RegularExpressions;

namespace src.Data;

public class SlideDataFile
{
    private Regex titlesPattern = new Regex(@"^Block\s*", RegexOptions.IgnoreCase);
    public SlideDataFile(string filename, string content)
    {
        Filename = filename;
        Content = content;
    }
    public SlideDataFile() { }
    public string Filename { get; set; } = "";
    public string Content { get; set; } = "";

    public string[] getTitles()
    {
        string[] allLines = Content.Split("\n");
        int beginningIndex = Array.FindIndex(allLines, line => titlesPattern.Match(line).Success);
        return allLines[beginningIndex].Split("\t");
    }
    public string[] getSpotLines()
    {
        string[] allLines = Content.Split("\n");
        int beginningIndex = Array.FindIndex(allLines, line => titlesPattern.Match(line).Success);
        return new ArraySegment<string>(allLines, beginningIndex + 1, allLines.Length - beginningIndex - 2).ToArray();
    }
}