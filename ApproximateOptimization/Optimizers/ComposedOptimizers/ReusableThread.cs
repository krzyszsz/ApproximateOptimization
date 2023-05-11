using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

/// <summary>
/// Custom thread pool. This is not using the built in .net thread pool to not impact other tasks.
/// </summary>
internal sealed class ReusableThread
{
    private static ConcurrentStack<(Thread, SemaphoreSlim, AutoResetEvent, ReferenceToActionsQueue)> threads = new();
    private static CancellationTokenSource cts = new();
    private SemaphoreSlim _startSemaphore;
    private Thread _thread;
    private ReferenceToActionsQueue _referenceToActionsQueue;
    public AutoResetEvent FinishSignal;

    public ReusableThread(ConcurrentQueue<Action> actions)
    {
        if (threads.TryPop(out var threadData))
        {
            (_thread, _startSemaphore, FinishSignal, _referenceToActionsQueue) = threadData;
            _referenceToActionsQueue.Ref = actions;
        }
        else
        {
            _startSemaphore = new SemaphoreSlim(0, 1);
            FinishSignal = new AutoResetEvent(false);
            _referenceToActionsQueue = new ReferenceToActionsQueue()
            {
                Ref = actions
            };
            _thread = new Thread(new ParameterizedThreadStart((object threadParameter) =>
            {
                (SemaphoreSlim startSemaphore, AutoResetEvent finishSignal, ReferenceToActionsQueue referenceToActionsQueue) = threadParameter as Tuple<SemaphoreSlim, AutoResetEvent, ReferenceToActionsQueue>;
                var token = cts.Token;
                while (!cts.IsCancellationRequested)
                {
                    startSemaphore.Wait();
                    if (!token.IsCancellationRequested)
                    {
                        while (referenceToActionsQueue.Ref.TryDequeue(out var localAction)) localAction();
                        Thread.MemoryBarrier();
                    }
                    finishSignal.Set();
                }
            }));
            _thread.Start(new Tuple<SemaphoreSlim, AutoResetEvent, ReferenceToActionsQueue>(_startSemaphore, FinishSignal, _referenceToActionsQueue));
        }
    }

    public void Start()
    {
        _startSemaphore.Release();
    }

    public void Join()
    {
        // This is not the same as Join() from standard thread: first of all it MUST be called each time (otherwise thread is not returned to the pool).
        FinishSignal.WaitOne();
        InternalJoin();
    }

    internal void InternalJoin()
    {
        threads.Push((_thread, _startSemaphore, FinishSignal, _referenceToActionsQueue));
    }

    public static void Destroy()
    {
        // Not a standard Dispose because this method removes static objects and should only be called when optimizers will no longer be needed.
        cts.Cancel();
        while (threads.Count > 0)
        {
            threads.TryPop(out var t);
            t.Item2.Dispose(); // Make sure all threads are Joined before this is called.
            t.Item3.Dispose(); // Make sure all threads are Joined before this is called.
        }
    }

    internal sealed class ReferenceToActionsQueue
    {
        public ConcurrentQueue<Action> Ref { get; set; }
    }

}

public sealed class ParallelForEach<T>
{
    private readonly ReusableThread[] _threads;
    private ConcurrentQueue<Action> _actions = new();

    public ParallelForEach(int maxThreads, List<T> items, Action<T> action)
    {
        var threadsNumber = Math.Min(maxThreads, items.Count);
        _threads = new ReusableThread[threadsNumber];
        for (var i = 0; i < items.Count; i++) EnqueueBeforeStart(getForEachAction(action, items[i]));
        for (var i = 0; i < threadsNumber; i++)
        {
            _threads[i] = new ReusableThread(_actions);
        }
        for (var i = 0; i < threadsNumber; i++) _threads[i].Start();
    }

    private void EnqueueBeforeStart(Action action)
    {
        _actions.Enqueue(action);
    }

    private Action getForEachAction(Action<T> action, T x)
    {
        return () => action(x);
    }

    public void Join()
    {
        if (_threads.Length <= 64)
        {
            WaitHandle.WaitAll(_threads.Select(x => x.FinishSignal).ToArray());
        }
        else
        {
            for (var i = 0; i < _threads.Length; i++)
            {
                _threads[i].FinishSignal.WaitOne();
            }
        }
        for (var i = 0; i < _threads.Length; i++)
        {
            _threads[i].InternalJoin();
        }
    }
}