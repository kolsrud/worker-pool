# worker-pool
Basic utility library for handling a pool for concurrent jobs. A typical usage could look like this:

```
// Run four jobs concurrently.
var workerPool = new WorkerPool.WorkerPool(4);
workerPool.AddWork(jobs);
while (workerPool.HasWork)
{
    workerPool.GetResult();
}
```
Examples can be found here: https://github.com/kolsrud/worker-pool/tree/main/src/Examples
