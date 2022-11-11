﻿using System.Drawing;

namespace src.Data;

public class Block
{
    public Block(Dictionary<string, string> patientData, List<Nplicate> nplicates, Color textColour, double qc)
    {
        PatientData = patientData;
        Nplicates = nplicates;
        TextColour = textColour;
        QC = qc;
    }
    public Block(Dictionary<string, string> patientData)
    {
        PatientData = patientData;
    }

    public List<Nplicate> Nplicates { get; set; } = new List<Nplicate>();
    public Dictionary<string, string> PatientData { get; set; }
    public Color TextColour { get; set; } = new Color();
    public double QC { get; private set; } = 0;

    public void CalculateQC(Nplicate pos, Nplicate neg)
    {
        QC = pos.Mean == 0 ? double.NaN : (pos.Mean - neg.Mean) / pos.Mean;
    }
}