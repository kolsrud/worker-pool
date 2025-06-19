using System.Diagnostics;

namespace HelloWorld
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var delays = new [] { 8, 1, 4, 5, 2, 7, 6, 3 };
            var sw = new Stopwatch();
            var jobs = delays.Select((n, i) => (Func<Task>) (() => HelloWorld(i, n, sw))).ToArray();
            const int workerCnt = 4;
            var workerPool = new WorkerPool.WorkerPool(workerCnt);
            Console.WriteLine($"Created worker pool with {workerCnt} workers.");
            sw.Start();
            workerPool.AddWork(jobs);
            while (workerPool.HasWork)
            {
                workerPool.GetResult();
            }
        }

        static async Task HelloWorld(int i, int delay, Stopwatch sw)
        {
            Console.WriteLine($"Running HelloWorld job {i} with delay {delay}s starting at {sw.Elapsed}.");
            await Task.Delay(TimeSpan.FromSeconds(delay));
            Console.WriteLine($"Running HelloWorld job {i} with delay {delay}s completed at {sw.Elapsed}.");
        }
    }
}
