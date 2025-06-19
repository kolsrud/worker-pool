using System.Collections.Concurrent;

namespace WorkerPool
{
    public interface IWorkerPool
    {
        bool IsFull { get; }
        bool HasWork { get; }
        int CurrentLoad { get; }

        void AddWork(Func<Task> job);
        void AddWork(IEnumerable<Func<Task>> jobs);
        Task GetResult();
    }

    public interface IWorkerPool<T>
    {
        bool IsFull { get; }
        bool HasWork { get; }
        int CurrentLoad { get; }

        void AddWork(Func<Task<T>> job);
        void AddWork(IEnumerable<Func<Task<T>>> jobs);
        Task<T> GetResult();
    }

    public class WorkerPool<T> : WorkerPoolGeneric, IWorkerPool<T>
    {
        public WorkerPool(int workerCount) : base(workerCount)
        {
        }

        public void AddWork(Func<Task<T>> job)
        {
            AddGenericWork(job);
        }

        public void AddWork(IEnumerable<Func<Task<T>>> jobs)
        {
            AddGenericWork(jobs);
        }

        public Task<T> GetResult()
        {
            return (Task<T>)GetGenericResult();
        }
    }

    public class WorkerPool : WorkerPoolGeneric, IWorkerPool
    {
        public WorkerPool(int workerCount) : base(workerCount)
        {
        }

        public void AddWork(IEnumerable<Func<Task>> jobs)
        {
            AddGenericWork(jobs);
        }

        public void AddWork(Func<Task> job)
        {
            AddGenericWork(job);
        }

        public Task GetResult()
        {
            return GetGenericResult();
        }
    }

    public class WorkerPoolGeneric
    {
        private readonly int _workerCount;
        private int _freeWorkers;

        private readonly Dictionary<int, Task> _workers = new Dictionary<int, Task>();
        private readonly Queue<Func<Task>> _queue = new Queue<Func<Task>>();
        private readonly BlockingCollection<Task> _completedTasks = new BlockingCollection<Task>();

        public WorkerPoolGeneric(int workerCount)
        {
            if (workerCount <= 0) throw new ArgumentException("Number of workers must be greater than 0.", nameof(workerCount));

            _workerCount = workerCount;
            _freeWorkers = workerCount;
        }

        private static int _taskId = 0;

        protected void AddGenericWork(IEnumerable<Func<Task>> jobs)
        {
            foreach (var job in jobs)
            {
                AddGenericWork(job);
            }
        }

        protected void AddGenericWork(Func<Task> job)
        {
            lock (_workers)
            {
                if (_freeWorkers == 0)
                {
                    _queue.Enqueue(job);
                    return;
                }

                var id = _taskId++;
                _workers.Add(id, job().ContinueWith(t => OnTaskCompleted(id, t)));
                _freeWorkers--;
            }
        }

        private void OnTaskCompleted(int id, Task t)
        {
            lock (_workers)
            {
                _workers.Remove(id);

                _freeWorkers++;
                if (_queue.Any())
                {
                    var job = _queue.Dequeue();
                    AddGenericWork(job);
                }
            }

            _completedTasks.Add(t);
        }

        public bool IsFull => _freeWorkers == 0;
        public bool HasWork => _freeWorkers != _workerCount || _completedTasks.Any();
        public int CurrentLoad => _queue.Count + (_workerCount - _freeWorkers);

        protected Task GetGenericResult()
        {
            if (!HasWork) throw new Exception("No tasks running.");

            return _completedTasks.Take();
        }
    }
}