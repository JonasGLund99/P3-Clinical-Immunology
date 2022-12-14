using Microsoft.Azure.Cosmos;
namespace src.Data;

public class ProcessQueue
{
    private static readonly ProcessQueue instance = new ProcessQueue();
    private Queue<Func<Task>> queue = new Queue<Func<Task>>();
    private int queueCount = 0;
    public bool isRunning = false;


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
        queue.Enqueue(process);
        queueCount++;

        if (!isRunning)
        {
            queueCount = 1;
            Execute();
        }
    }
    private async void Execute()
    {
        isRunning = true;

        Func<Task> currentProcess;

        do
        {
            currentProcess = queue.Dequeue();
            await currentProcess();
        }
        while (queue.Count > 0);

        isRunning = false;
    }

    public double GetProgress()
    {
        if (!isRunning || queueCount == 0) return 1;
        return 1 - (double)queue.Count / (double)queueCount;
    }
    public void Clear(string queueId)
    {
        queue.Clear();
        queueCount = 0;
    }
}