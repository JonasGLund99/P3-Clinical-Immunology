using src.Data;
using Xunit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Tests.old;


public class ExperimentTest
{
    private readonly Experiment _sut;

    public ExperimentTest()
    {
        _sut = new Experiment();
    }

    public class ExperimentTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { generateExperiment() };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private Experiment generateExperiment()
        {
            string author = "Rikke Bæk";

            Experiment e = new Experiment(
            id: Guid.NewGuid().ToString(),
            experimentNumber: $"TESTEXP-1",
            // title: $"Experiment {i + 1}",
            title: "TestExperiment 1",
            author: author,
            description: "TESTSome description",
            createdAt: DateTime.Now
            );

            return e;

        }
    }
}
