using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using OfficeOpenXml.Core;
using src.Data;
using System.Collections;
using Xunit;

namespace src.Tests;

//https://youtu.be/2Wp8en1I9oQ?t=1152 Se delen om Reusing Instances
public class DisassociateTest
{
    [Fact]
    public async void DisassociateRemovesExperimentAndClinicalTestid()
    {
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for SaveToDatabase test in ClinicalTest");

        Container ctContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("ClinicalTest", "/PartitionKey");

        string guidClinicalTest1 = Guid.NewGuid().ToString();
        ClinicalTest c1 = new ClinicalTest { id = guidClinicalTest1, PartitionKey = guidClinicalTest1 };

        string guidExperiment1 = Guid.NewGuid().ToString();
        Experiment e1 = new Experiment { id = guidExperiment1, PartitionKey = guidExperiment1 };

        c1.ExperimentIds.Add(guidExperiment1);
        e1.ClinicalTestIds.Add(guidClinicalTest1);

        // Act
        await ExperimentManager.Disassociate(e1, c1);

        Assert.True(!e1.ClinicalTestIds.Exists(id => id == guidClinicalTest1));
        Assert.True(!c1.ExperimentIds.Exists(id => id == guidExperiment1));

        await ctContainer.DeleteContainerAsync();
    }

    [Fact]
    public async void DisassociateRemovesClinicalTestWithEmptyExperimentids()
    {
        DatabaseService.EnableTestMode();
        await DatabaseService.Instance.SetupDatabase();
        if (DatabaseService.Instance.Database == null) throw new Exception("Database did not complete setup for SaveToDatabase test in ClinicalTest");

        string guidClinicalTest1 = Guid.NewGuid().ToString();
        ClinicalTest c1 = new ClinicalTest { id = guidClinicalTest1, PartitionKey = guidClinicalTest1 };

        string guidExperiment1 = Guid.NewGuid().ToString();
        Experiment e1 = new Experiment { id = guidExperiment1, PartitionKey = guidExperiment1 };

        Container ctContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("ClinicalTest", "/PartitionKey");

        // Act
        await ExperimentManager.Associate(e1, c1);
        Assert.True(e1.ClinicalTestIds.Exists(id => id == guidClinicalTest1));
        Assert.True(c1.ExperimentIds.Exists(id => id == guidExperiment1));

        await ExperimentManager.Disassociate(e1, c1);



        await Assert.ThrowsAnyAsync<CosmosException>(() => ctContainer.ReadItemAsync<ClinicalTest>(c1.id, new PartitionKey(c1.PartitionKey)));

        await ctContainer.DeleteContainerAsync();

    }

}


