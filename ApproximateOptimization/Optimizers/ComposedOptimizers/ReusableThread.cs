using System;
using System.Collections.Concurrent;
using System.Threading;
/// <summary>
/// Simple thread pool. This is not using the built in .net thread pool to not impact other tasks.
/// </summary>
public sealed class ReusableThread
{
    private static ConcurrentQueue<(Thread, SemaphoreSlim, ManualResetEvent)> threads = new();
    private static ConcurrentQueue<Action> actions = new();
    private static CancellationTokenSource cts = new();
    private ManualResetEvent _finishMRE;
    private SemaphoreSlim _startSemaphore;
    private Thread _thread;

    public ReusableThread(Action action)
    {
        actions.Enqueue(action);
        if (threads.TryDequeue(out var threadData))
        {
            (_thread, _startSemaphore, _finishMRE) = threadData;
        }
        else
        {
            _startSemaphore = new SemaphoreSlim(0);
            _finishMRE = new ManualResetEvent(false);
            _thread = new Thread(new ParameterizedThreadStart((object threadParameter) =>
            {
                (SemaphoreSlim startSemaphore, ManualResetEvent finishMRE) = (Tuple<SemaphoreSlim, ManualResetEvent>)threadParameter;
                var token = cts.Token;
                while (!cts.IsCancellationRequested)
                {
                    startSemaphore.Wait(token);
                    if (actions.TryDequeue(out var localAction)) localAction();
                    finishMRE.Set();
                }
            }));
            _thread.IsBackground = true;
            _thread.Start(new Tuple<SemaphoreSlim, ManualResetEvent>(_startSemaphore, _finishMRE));
        }
    }

    public void Start()
    {
        _startSemaphore.Release();
    }

    public void Join()
    {
        // This is not the same as Join() from standard thread: first of all it MUST be called each time (otherwise thread is not returned to the pool).
        _finishMRE.WaitOne();
        _finishMRE.Reset();
        threads.Enqueue((_thread, _startSemaphore, _finishMRE));
        _startSemaphore = null;
        _finishMRE = null;
        _thread = null;
    }

    public static void Destroy()
    {
        // Not a standard Dispose because this method removes static objects and should only be called when optimizers will no longer be needed.
        cts.Cancel();
        while (actions.Count > 0) actions.TryDequeue(out var _);
        while (threads.Count > 0)
        {
            threads.TryDequeue(out var t);
            t.Item2.Dispose(); // Make sure all threads are Joined before this is called.
            t.Item3.Dispose(); // Make sure all threads are Joined before this is called.
        }
    }
}