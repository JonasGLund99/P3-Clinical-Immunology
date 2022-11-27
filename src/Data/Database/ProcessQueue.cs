using Microsoft.Azure.Cosmos;
namespace src.Data;

class ProcessQueue
{
    private static readonly ProcessQueue instance = new ProcessQueue();
    private Dictionary<string, Queue<Func<Task>>> queues = new Dictionary<string, Queue<Func<Task>>>();
    private Dictionary<string, int> queueCounts = new Dictionary<string, int>();
    public Dictionary<string, bool> isRunning = new Dictionary<string, bool>();


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
            queueCounts[queueId] = 1;
        }
        queues[queueId].Enqueue(process);
        queueCounts[queueId]++;

        if (!isRunning.ContainsKey(queueId) || !isRunning[queueId])
        {
            queueCounts[queueId] = 1;
            Execute(queueId);
        }
    }
    private async void Execute(string queueId)
    {
        isRunning[queueId] = true;

        Func<Task> currentProcess;

        do
        {
            currentProcess = queues[queueId].Dequeue();
            await currentProcess();
        }
        while (queues[queueId].Count > 0);

        isRunning[queueId] = false;
    }

    public double GetProgress(string queueId)
    {
        if (!isRunning.ContainsKey(queueId) || !isRunning[queueId] || queueCounts[queueId] == 0) return 1;
        return 1 - (double)queues[queueId].Count / (double)queueCounts[queueId];
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
            queueCounts[queueId] = 0;
        }
    }
}