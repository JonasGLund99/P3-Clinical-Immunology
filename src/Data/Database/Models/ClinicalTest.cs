﻿using System.Text.RegularExpressions;

namespace src.Data;

public class ClinicalTest : BaseModel<ClinicalTest>
{
    private int nplicatesInBlock { get; set; }

    public ClinicalTest(string id, string title, int nplicateSize, string description, DateTime createdAt, DateTime editedAt, List<SlideDataFile> slideDataFiles, Dictionary<string, bool> patientKeys, List<string> activeKeys, List<string> experimentIds, List<Slide> slides, List<string> analyteNames) : base(id)
    {
        Title = title;
        NplicateSize = nplicateSize;
        Description = description;
        CreatedAt = createdAt;
        EditedAt = editedAt;
        SlideDataFiles = slideDataFiles;
        PatientKeys = patientKeys;
        ActiveKeys = activeKeys;
        ExperimentIds = experimentIds;
        Slides = slides;
        AnalyteNames = analyteNames;
    }
    public ClinicalTest(string id) : base(id) { }

    public List<SlideDataFile> SlideDataFiles { get; set; } = new List<SlideDataFile>();
    public Dictionary<string, bool> PatientKeys { get; set; } = new Dictionary<string, bool>();
    public List<string> ActiveKeys { get; set; } = new List<string>();
    public List<string> ExperimentIds { get; set; } = new List<string>();
    public List<Slide> Slides { get; set; } = new List<Slide>();
    public List<string> AnalyteNames { get; set; } = new List<string>();
    public string Title { get; set; } = "";
    public int NplicateSize { get; set; } = 3;
    public string Description { get; set; } = "";
    public double MaxRI { get; private set; } = 0;
    public double MinRI { get; private set; } = 0;
    public DateTime CreatedAt { get; } = DateTime.Now;
    public DateTime EditedAt { get; private set; } = DateTime.Now;

    public void AddSlide(Slide slide, List<Dictionary<string, string>> patientData)
    {
        int numOfBlocks = 21;
        Slides.Add(slide);
        for (int i = 0; i < numOfBlocks; i++)
        {
            slide.Blocks.Add(new Block(patientData[i]));
        }
    }

    public void CreatePatientKeys(List<string> allKeys, params string[] shownKeys)
    {
        PatientKeys.Clear();
        ActiveKeys.Clear();

        foreach (string key in allKeys)
        {
            PatientKeys.Add(key, false);
        }

        foreach (string key in shownKeys)
        {
            PatientKeys[key] = true;
            ActiveKeys.Add(key);
        }
    }

    private void UpdatePatientKeys(params string[] newKeys)
    {
        foreach (string key in ActiveKeys)
        {
            PatientKeys[key] = false;
        }
        ActiveKeys.Clear();

        foreach (string key in newKeys)
        {
            PatientKeys[key] = true;
            ActiveKeys.Add(key);
        }        
    }

    public void ExportClinicalTest(string exportType)
    {
        
    }

    private void calculateClinicalTestResult()
    {
        int beginningIndex = 0;
        Regex start = new Regex(@"^Block\s*Row\s*Column\s*Name\s*ID", RegexOptions.IgnoreCase);
        for (int i = 0; i < SlideDataFiles.Count; i++) 
        {
            //Read all lines in a file and add each line as an element in a string array
            string[] allLines = SlideDataFiles[i].Content.Split("\n");

            //Find the line in which the information about spots begin
            beginningIndex = Array.FindIndex(allLines, line => start.Match(line).Success);

            //Create string array with only spot information
            string[] spotLines = new string[allLines.Length - 1 - beginningIndex];
            allLines.CopyTo(spotLines, beginningIndex + 1);
            
            //Create array where each entry is a title for spot information
            string[] titles = allLines[beginningIndex].Split("\t");

            List<string> spotInfo = new List<string>();
            nplicatesInBlock = spotLines.Length / NplicateSize;

            for (int j = 0; j < Slides[i].Blocks.Count; j++)
            {
                for (int k = 0; k < nplicatesInBlock; k++)
                {
                    //Split the line with spotinformation, add the information elements to spotinfo.
                    spotInfo.AddRange(spotLines[k * NplicateSize + (j * nplicatesInBlock * NplicateSize)].Split("\t"));

                    //Find the index in spotInfo that contains the analyteType (ID) and create an Nplicate with it.
                    Nplicate nplicate = new Nplicate(spotInfo[Array.IndexOf(titles, "Id")]);

                    // Add analyteNames when looping through the first block
                    if (j == 0) {
                        AnalyteNames.Add(findSingleSpotInfo(spotInfo, titles, "Name"));
                    }

                    for (int l = 0; l < NplicateSize; l++)
                    {                        
                        if (l != 0)
                        {
                            spotInfo.AddRange(spotLines[l + (k * NplicateSize) + (j * nplicatesInBlock * NplicateSize)].Split("\t"));
                        }

                        nplicate.Spots.Add(
                            new Spot(
                                intensity: double.Parse(findSingleSpotInfo(spotInfo, titles, "Intensity")), 
                                flagged: findSingleSpotInfo(spotInfo, titles, "Flags") != "0"
                            )
                        );
                        spotInfo.Clear();
                    }
                    //Calculate the mean and set if the Nplicate are flagged.
                    nplicate.CalculateMean();
                    nplicate.SetFlag();

                    Slides[i].Blocks[j].Nplicates.Add(nplicate);
                }
                Nplicate? pos = Slides[i].Blocks[j].Nplicates.Find(nplicate => nplicate.AnalyteType == "pos");
                Nplicate? neg = Slides[i].Blocks[j].Nplicates.Find(nplicate => nplicate.AnalyteType == "neg");

                //Calculate the Quality control if the positive and negative control exist
                if (pos == null || neg == null)
                {
                    throw new NullReferenceException("Either the positive or negative control is missing");
                }
                Slides[i].Blocks[j].CalculateQC(pos, neg);
            }

            //Calculate the RI for each Nplicate in each block and update max / min RI
            foreach (Block block in Slides[i].Blocks)
            {
                Nplicate? neg = block.Nplicates.Find(element => element.AnalyteType == "neg");
                
                if (neg == null)
                {
                    throw new NullReferenceException("The negative control is missing");
                }

                for (int j = 0; j < block.Nplicates.Count; j++)
                {
                    updateMaxMinRI(block.Nplicates[j].CalculateRI(Slides[i].Blocks[Slides[i].Blocks.Count - 1].Nplicates[j], neg));
                }
            }
        }

        //Set the heatmapcolour of all Nplicates (The RI of all Nplicates in all slides must be calculated before)
        foreach (Slide slide in Slides)
        {
            foreach (Block block in slide.Blocks)
            {
                foreach (Nplicate nplicate in block.Nplicates)
                {
                    nplicate.SetHeatMapColour(MaxRI, MinRI);
                }
            }
        }
    }

    private string findSingleSpotInfo(List<string> spotInfo, string[] titles, string key) 
    {
        return spotInfo[Array.IndexOf(titles, Array.Find(titles, element => element.Contains(key)))];
    }

    private void updateMaxMinRI(double RI)
    {
        if (RI > MaxRI)
        {
            MaxRI = RI;
        }
        else if (RI < MinRI)
        {
            MinRI = RI;
        }
    }
}


