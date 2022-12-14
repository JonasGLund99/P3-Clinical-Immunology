using Xunit;
using src.Data;

namespace src.Tests;

public class GetProgressTest
{
    [Fact]
    public void ProgressLessThanOneWhileRunning()
    {
        Func<Task> process = () => Task.Delay(100);
        string queueId = Guid.NewGuid().ToString();
        ProcessQueue.Instance.Enqueue(process, queueId);
        ProcessQueue.Instance.Enqueue(process, queueId);
        ProcessQueue.Instance.Enqueue(process, queueId);

        while (ProcessQueue.Instance.IsRunning[queueId])
        {
            Assert.True(ProcessQueue.Instance.GetProgress(queueId) <= 1);
        }
        Assert.True(ProcessQueue.Instance.GetProgress(queueId) == 1);
    }
}