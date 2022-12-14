using Microsoft.Azure.Cosmos;
namespace src.Data;

public class ProcessQueue
{
    private static readonly ProcessQueue instance = new ProcessQueue();
    public int QueueCount = 0;
    public Queue<Func<Task>> Queue { get; } = new Queue<Func<Task>>();
    public bool IsRunning { get; private set; } = false;


    private ProcessQueue() { }

    public static ProcessQueue Instance
    {
        get 
        {
            return instance;
        }
    }

    public void Enqueue(Func<Task> process)
    {
        Queue.Enqueue(process);
        QueueCount++;

        if (!IsRunning)
        {
            QueueCount = 1;
            Execute();
        }
    }
    private async void Execute()
    {
        IsRunning = true;

        Func<Task> currentProcess;

        do
        {
            currentProcess = Queue.Dequeue();
            await currentProcess();
        }
        while (Queue.Count > 0);

        IsRunning = false;
    }

    public double GetProgress()
    {
        if (!IsRunning || QueueCount == 0) return 1;
        return 1 - (double)Queue.Count / (double)QueueCount;
    }
    public void Clear()
    {
        Queue.Clear();
        QueueCount = 0;
    }
}