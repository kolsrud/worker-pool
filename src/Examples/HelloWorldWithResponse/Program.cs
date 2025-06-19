using System.Diagnostics;

namespace HelloWorldWithResponse
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var delays = new[] { 8, 1, 4, 5, 2, 7, 6, 3 };
            var sw = new Stopwatch();
            var jobs = delays.Select((n, i) => (Func<Task<int>>)(() => HelloWorld(i, n, sw))).ToArray();
            const int workerCnt = 4;
            var workerPool = new WorkerPool.WorkerPool<int>(workerCnt);
            Console.WriteLine($"Created worker pool with {workerCnt} workers.");
            sw.Start();
            workerPool.AddWork(jobs);
            var orderOfCompletion = new List<int>();
            while (workerPool.HasWork)
            {
                var i = workerPool.GetResult().Result;
                orderOfCompletion.Add(i);
            }

            Console.WriteLine("Order of completion: " + string.Join(", ", orderOfCompletion));
        }

        static async Task<int> HelloWorld(int i, int delay, Stopwatch sw)
        {
            Console.WriteLine($"Running HelloWorld job {i} with delay {delay}s starting at {sw.Elapsed}.");
            await Task.Delay(TimeSpan.FromSeconds(delay));
            Console.WriteLine($"Running HelloWorld job {i} with delay {delay}s completed at {sw.Elapsed}.");
            return i;
        }
    }
}
