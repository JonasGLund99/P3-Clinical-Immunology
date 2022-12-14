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
        ProcessQueue.Instance.Clear();
        ProcessQueue.Instance.Enqueue(process);
        Assert.True(ProcessQueue.Instance.IsRunning);
    }

    [Fact]
    public async void IsNotRunning()
    {
        Func<Task> process = () => Task.Delay(100);
        string queueId = Guid.NewGuid().ToString();
        ProcessQueue.Instance.Clear();
        ProcessQueue.Instance.Enqueue(process);
        await Task.Delay(200);
        Assert.False(ProcessQueue.Instance.IsRunning);
    }
}


