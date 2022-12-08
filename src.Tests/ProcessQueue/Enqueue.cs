using Xunit;
using src.Data;
using System.Collections;

namespace src.Tests;

public class EnqueueTest
{
    [Fact]
    public void IsRunning()
    {
        Func<Task> process = () => Task.Delay(1000);
        string queueId = Guid.NewGuid().ToString();
        ProcessQueue.Instance.Clear(queueId);
        ProcessQueue.Instance.Enqueue(process, queueId);
        Assert.True(ProcessQueue.Instance.IsRunning[queueId]);
    }

    [Fact]
    public async void IsNotRunning()
    {
        Func<Task> process = () => Task.Delay(1000);
        string queueId = Guid.NewGuid().ToString();
        ProcessQueue.Instance.Clear(queueId);
        ProcessQueue.Instance.Enqueue(process, queueId);
        await Task.Delay(1000);
        Assert.False(ProcessQueue.Instance.IsRunning[queueId]);
    }

    [Theory]
    [ClassData(typeof(EnqueueTestData))]
    public void QueueCount(int actualCount, int expectedCount)
    {
        Assert.True(actualCount == expectedCount);
    }

    class EnqueueTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            Func<Task> process = () => Task.Delay(1000);

            string queueId1 = Guid.NewGuid().ToString();
            ProcessQueue.Instance.Enqueue(process, queueId1);
            int actualCount1 = ProcessQueue.Instance.QueueCounts[queueId1];

            string queueId2 = Guid.NewGuid().ToString();
            ProcessQueue.Instance.Enqueue(process, queueId2);
            ProcessQueue.Instance.Enqueue(process, queueId2);
            int actualCount2 = ProcessQueue.Instance.QueueCounts[queueId2];

            string queueId3 = Guid.NewGuid().ToString();
            ProcessQueue.Instance.Enqueue(process, queueId3);
            ProcessQueue.Instance.Enqueue(process, queueId3);
            ProcessQueue.Instance.Enqueue(process, queueId3);
            ProcessQueue.Instance.Enqueue(process, queueId3);
            ProcessQueue.Instance.Enqueue(process, queueId3);
            int actualCount3 = ProcessQueue.Instance.QueueCounts[queueId3];

            yield return new object[] { actualCount1, 1 };
            yield return new object[] { actualCount2, 2 };
            yield return new object[] { actualCount3, 5 };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}


