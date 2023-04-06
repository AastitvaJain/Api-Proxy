namespace Interview
{
    public class Program
    {
        internal static async Task Main()
        {
            int numberOfTests = 100;
            int maxThreads = 3;
            SemaphoreSlim semaphore = new SemaphoreSlim(maxThreads);
            var proxyServer = new Proxy();

            Task[] tasks = new Task[numberOfTests];
            for (int i = 0; i < numberOfTests; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        Console.WriteLine($"Calling proxy on thread {Thread.CurrentThread.ManagedThreadId}");
                        // Call your test function here
                        await proxyServer.RunAsync();
                    }
                    finally
                    {
                        Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} excution completed");
                        semaphore.Release();
                    }
                });
            }

            Task.WaitAll(tasks);
            Console.WriteLine("All tests completed.");

            proxyServer.PrintStats();
        }
    }

}


