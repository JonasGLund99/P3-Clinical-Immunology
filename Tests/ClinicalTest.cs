using src.Data;
using Xunit;

namespace Tests;

public class Test_ClinicalTest
{
    [Fact]
    public void Test_CreatePatientKeys()
    {
        ClinicalTest ct = new ClinicalTest(
            id: "1",
            title: "Testing ClinicalTest",
            nplicateSize: 3,
            description: "Testing ClinicalTest",
            createdAt: DateTime.Now,
            editedAt: DateTime.Now,
            slideDataFiles: new List<SlideDataFile>(),
            patientKeys: new Dictionary<string, bool>(),
            activeKeys: new List<string>(),
            nplicatesInBlock: 72,
            experimentIds: new List<string>(),
            slides: new List<Slide>(),
            analyteNames: new List<string>()
        );

        ct.CreatePatientKeys(
            allKeys: new List<string>() { "age", "SoT", "isDead", "height" },
            "age",
            "height"
        );

        Assert.True(ct.PatientKeys["age"] && ct.PatientKeys["height"] && !ct.PatientKeys["SoT"] && !ct.PatientKeys["isDead"]);
    }
}
