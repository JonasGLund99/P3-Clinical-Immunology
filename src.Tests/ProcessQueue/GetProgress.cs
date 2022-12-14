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
        ProcessQueue.Instance.Enqueue(process);
        ProcessQueue.Instance.Enqueue(process);
        ProcessQueue.Instance.Enqueue(process);

        while (ProcessQueue.Instance.IsRunning)
        {
            Assert.True(ProcessQueue.Instance.GetProgress() <= 1);
        }
        Assert.True(ProcessQueue.Instance.GetProgress() == 1);
    }
}