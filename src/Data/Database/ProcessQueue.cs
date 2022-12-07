using Microsoft.Azure.Cosmos;
namespace src.Data;

public class ProcessQueue
{
    private static readonly ProcessQueue instance = new ProcessQueue();
    private Dictionary<string, Queue<Func<Task>>> queues = new Dictionary<string, Queue<Func<Task>>>();
    public Dictionary<string, int> QueueCounts = new Dictionary<string, int>();
    public Dictionary<string, bool> IsRunning = new Dictionary<string, bool>();


    private ProcessQueue() { }

    public static ProcessQueue Instance
    {
        get 
        {
            return instance;
        }
    }

    public void Enqueue(Func<Task> process, string queueId)
    {
        if (!queues.ContainsKey(queueId))
        {
            queues[queueId] = new Queue<Func<Task>>();
        }
        if (!QueueCounts.ContainsKey(queueId))
        {
            QueueCounts[queueId] = 1;
        }
        
        queues[queueId].Enqueue(process);
        QueueCounts[queueId]++;

        if (!IsRunning.ContainsKey(queueId) || !IsRunning[queueId])
        {
            QueueCounts[queueId] = 1;
            execute(queueId);
        }
    }
    private async void execute(string queueId)
    {
        IsRunning[queueId] = true;

        Func<Task> currentProcess;

        do
        {
            currentProcess = queues[queueId].Dequeue();
            await currentProcess();
        }
        while (queues[queueId].Count > 0);

        IsRunning[queueId] = false;
    }

    public double GetProgress(string queueId)
    {
        if (!IsRunning.ContainsKey(queueId) || !IsRunning[queueId] || QueueCounts[queueId] == 0) return 1;
        return 1 - (double)queues[queueId].Count / (double)QueueCounts[queueId];
    }
    public void Clear(string queueId)
    {
        if (!queues.ContainsKey(queueId))
        {
            queues[queueId] = new Queue<Func<Task>>();
        }
        else
        {
            queues[queueId].Clear();
            QueueCounts[queueId] = 0;
        }
        IsRunning[queueId] = false;
    }
    public Dictionary<string, Queue<Func<Task>>> GetQueues()
    {
        return queues;
    }
}