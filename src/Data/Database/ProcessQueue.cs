using Microsoft.Azure.Cosmos;
namespace src.Data;

class ProcessQueue
{
    private static readonly ProcessQueue instance = new ProcessQueue();
    private Dictionary<string, Queue<Func<Task>>> queues = new Dictionary<string, Queue<Func<Task>>>();
    private Dictionary<string, int> queueCounts = new Dictionary<string, int>();

    private ProcessQueue() { }

    public static ProcessQueue Instance
    {
        get 
        {
            return instance;
        }
    }

    public bool IsRunning = false;


    public void Enqueue(Func<Task> process, string queueId)
    {
        if (!queues.ContainsKey(queueId))
        {
            queues[queueId] = new Queue<Func<Task>>();
            queueCounts[queueId] = 1;
        }
        queues[queueId].Enqueue(process);
        queueCounts[queueId]++;

        if (!IsRunning)
        {
            queueCounts[queueId] = 1;
            Execute(queueId);
        }

    }
    private async void Execute(string queueId)
    {
        IsRunning = true;

        Func<Task> currentProcess;

        do
        {
            currentProcess = queues[queueId].Dequeue();
            await currentProcess();
        }
        while (queues[queueId].Count > 0);

        IsRunning = false;
    }

    public double GetProgress(string queueId)
    {
        return 1 - (double)queues[queueId].Count / (double)queueCounts[queueId];
    }
}