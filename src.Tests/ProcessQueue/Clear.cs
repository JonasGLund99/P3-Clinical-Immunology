using Xunit;
using src.Data;

namespace src.Tests;

public class ClearTest
{
    [Fact]
    public void ClearExists()
    {
        // Arrange
        Func<Task> process = () => Task.Delay(100);
        string queueId = Guid.NewGuid().ToString();

        // Act
        ProcessQueue.Instance.Enqueue(process);
        ProcessQueue.Instance.Clear();

        // Assert
        Assert.True(ProcessQueue.Instance.Queue.Count == 0);
    }

    [Fact]
    public void ClearNotExists()
    {
        // Arrange
        string queueId = Guid.NewGuid().ToString();

        // Act
        ProcessQueue.Instance.Clear();

        // Assert
        Assert.True(ProcessQueue.Instance.Queue.Count == 0);
    }
}