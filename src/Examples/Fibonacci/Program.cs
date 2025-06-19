namespace Fibonacci
{
    internal class Program
    {
        private static readonly WorkerPool.WorkerPool<(int,int)> WorkerPool = new WorkerPool.WorkerPool<(int, int)>(8);

        static void Main(string[] args)
        {
            WorkerPool.AddWork(() => Fibonacci(10));

            var result = 0;
            while (WorkerPool.HasWork)
            {
                Console.WriteLine($"Workload: {WorkerPool.CurrentLoad}");
                var (f0, f1) = WorkerPool.GetResult().Result;
                if (f0 <= 1)
                    result += f0;
                else
                    WorkerPool.AddWork(() => Fibonacci(f0));

                if (f1 <= 1)
                    result += f1;
                else
                    WorkerPool.AddWork(() => Fibonacci(f1));
            }

            Console.WriteLine("Result is: " + result);
        }

        private static async Task<(int, int)> Fibonacci(int n)
        {
            if (n <= 1) throw new ArgumentException("Can't expand fib base case.",  nameof(n));
            Console.WriteLine($"Fib({n})");
            await Task.Delay(100);
            return (n - 1, n - 2);
        }
    }
}
