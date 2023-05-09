using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

    public ReusableThread()
    {
        if (threads.TryDequeue(out var threadData))
        {
            (_thread, _startSemaphore, _finishMRE) = threadData;
        }
        else
        {
            _startSemaphore = new SemaphoreSlim(0);
            _finishMRE = new ManualResetEvent(true);
            _thread = new Thread(new ParameterizedThreadStart((object threadParameter) =>
            {
                (SemaphoreSlim startSemaphore, ManualResetEvent finishMRE) = (Tuple<SemaphoreSlim, ManualResetEvent>)threadParameter;
                var token = cts.Token;
                while (!cts.IsCancellationRequested)
                {
                    startSemaphore.Wait(token);
                    while (actions.TryDequeue(out var localAction)) localAction();
                    finishMRE.Set();
                }
            }));
            _thread.Start(new Tuple<SemaphoreSlim, ManualResetEvent>(_startSemaphore, _finishMRE));
        }
    }

    public static void EnqueueBeforeStart(Action action)
    {
        actions.Enqueue(action);
    }

    public void Start()
    {
        _finishMRE.Reset();
        _startSemaphore.Release();
    }

    public void Join()
    {
        // This is not the same as Join() from standard thread: first of all it MUST be called each time (otherwise thread is not returned to the pool).
        _finishMRE.WaitOne();
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

    public sealed class ParallelForEach<T>
    {
        private readonly ReusableThread[] _threads;

        public ParallelForEach(int maxThreads, List<T> items, Action<T> action)
        {
            var threadsNumber = Math.Min(maxThreads, items.Count);
            _threads = new ReusableThread[threadsNumber];
            for (var i = 0; i < threadsNumber; i++)
            {
                _threads[i] = new ReusableThread();
            }
            for (var i = 0; i < items.Count; i++) ReusableThread.EnqueueBeforeStart(getForEachAction(action, items[i]));
            for (var i = 0; i < maxThreads; i++) _threads[i].Start();
        }

        private Action getForEachAction(Action<T> action, T x)
        {
            return () => action(x);
        }

        public void Join()
        {
            ManualResetEvent.WaitAll(_threads.Select(x => x._finishMRE).ToArray());
            for (var i = 0; i < _threads.Length; i++)
            {
                _threads[i].Join();
            }
        }
    }
}