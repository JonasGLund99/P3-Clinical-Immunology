using Xunit;
using Microsoft.Azure.Cosmos;
using src.Data;
using System.Collections;

namespace src.Tests;

public class EnqueueTest
{
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
            int actualCount1 = ProcessQueue.Instance.GetQueues()[queueId1].Count;

            string queueId2 = Guid.NewGuid().ToString();
            ProcessQueue.Instance.Enqueue(process, queueId2);
            ProcessQueue.Instance.Enqueue(process, queueId2);
            int actualCount2 = ProcessQueue.Instance.GetQueues()[queueId2].Count;

            string queueId3 = Guid.NewGuid().ToString();
            ProcessQueue.Instance.Enqueue(process, queueId3);
            ProcessQueue.Instance.Enqueue(process, queueId3);
            ProcessQueue.Instance.Enqueue(process, queueId3);
            ProcessQueue.Instance.Enqueue(process, queueId3);
            ProcessQueue.Instance.Enqueue(process, queueId3);
            int actualCount3 = ProcessQueue.Instance.GetQueues()[queueId3].Count;

            yield return new object[] { actualCount1, 0 };
            yield return new object[] { actualCount2, 1 };
            yield return new object[] { actualCount3, 4 };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}


