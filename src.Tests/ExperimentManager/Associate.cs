using src.Data;
using Xunit;

namespace src.Tests;

//https://youtu.be/2Wp8en1I9oQ?t=1152 Se delen om Reusing Instances
public class AssociateTest
{
    [Fact]
    public async void AssociateAddsExperimentAndClinicalTestid()
    {
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for SaveToDatabase test in ClinicalTest");

        string guidClinicalTest1 = Guid.NewGuid().ToString();
        ClinicalTest c1 = new ClinicalTest { id = guidClinicalTest1, PartitionKey = guidClinicalTest1 };

        string guidExperiment1 = Guid.NewGuid().ToString();
        Experiment e1 = new Experiment { id = guidExperiment1, PartitionKey = guidExperiment1 };

        // Act
        await ExperimentManager.Associate(e1, c1);

        Assert.True(e1.ClinicalTestIds.Exists(id => id == guidClinicalTest1));
        Assert.True(c1.ExperimentIds.Exists(id => id == guidExperiment1));
    }

    [Fact]
    public async void AssociateDoesNotAddExistingid()
    {
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for SaveToDatabase test in ClinicalTest");

        string guidClinicalTest1 = Guid.NewGuid().ToString();
        ClinicalTest c1 = new ClinicalTest { id = guidClinicalTest1, PartitionKey = guidClinicalTest1 };

        string guidExperiment1 = Guid.NewGuid().ToString();
        Experiment e1 = new Experiment { id = guidExperiment1, PartitionKey = guidExperiment1 };

        // Act
        await ExperimentManager.Associate(e1, c1);
        Assert.True(e1.ClinicalTestIds.Exists(id => id == guidClinicalTest1));
        Assert.True(c1.ExperimentIds.Exists(id => id == guidExperiment1));

        await ExperimentManager.Associate(e1, c1);

        int experimentIDCount = 0;

        foreach(string experimentID in c1.ExperimentIds)
        {
            if (experimentID == guidExperiment1)
            {
                experimentIDCount++;
            }
        }

        int clinicalTestIDCount = 0;

        foreach (string clinicalTestID in e1.ClinicalTestIds)
        {
            if (clinicalTestID == guidClinicalTest1)
            {
                clinicalTestIDCount++;
            }
        }

        Assert.Equal(1, experimentIDCount);
        Assert.Equal(1, clinicalTestIDCount);

    }

}


            