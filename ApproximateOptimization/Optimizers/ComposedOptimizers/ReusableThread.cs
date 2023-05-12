using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Custom thread pool. This is not using the built in .net thread pool to not impact other tasks.
/// </summary>
internal sealed class ReusableThread
{
    private static ConcurrentStack<(Thread, SemaphoreSlim, SemaphoreSlim, SemaphoreSlim, ReferenceToActionsQueue)> threads = new();
    private static CancellationTokenSource cts = new();
    private readonly SemaphoreSlim _startSemaphore;
    private readonly Thread _thread;
    private readonly ReferenceToActionsQueue _referenceToActionsQueue;
    public readonly SemaphoreSlim FinishSignal;
    public readonly SemaphoreSlim StoppedSignal;

    public ReusableThread(ConcurrentQueue<Action> actions)
    {
        if (threads.TryPop(out var threadData))
        {
            (_thread, _startSemaphore, FinishSignal, StoppedSignal, _referenceToActionsQueue) = threadData;
            _referenceToActionsQueue.Ref = actions;
        }
        else
        {
            _startSemaphore = new SemaphoreSlim(0, 1);
            FinishSignal = new SemaphoreSlim(0, 1);
            StoppedSignal = new SemaphoreSlim(0, 1);
            _referenceToActionsQueue = new ReferenceToActionsQueue()
            {
                Ref = actions
            };
            _thread = new Thread(new ParameterizedThreadStart((object threadParameter) =>
            {
                (SemaphoreSlim startSemaphore, SemaphoreSlim finishSignal, SemaphoreSlim stoppedSignal, ReferenceToActionsQueue referenceToActionsQueue) = threadParameter as Tuple<SemaphoreSlim, SemaphoreSlim, SemaphoreSlim, ReferenceToActionsQueue>;
                var token = cts.Token;
                while (!cts.IsCancellationRequested)
                {
                    startSemaphore.Wait();
                    if (!token.IsCancellationRequested)
                    {
                        while (referenceToActionsQueue.Ref.TryDequeue(out var localAction)) localAction();
                    }
                    finishSignal.Release();
                }
                stoppedSignal.Release();
            }));
            _thread.IsBackground = true;
            _thread.Start(new Tuple<SemaphoreSlim, SemaphoreSlim, SemaphoreSlim, ReferenceToActionsQueue>(_startSemaphore, FinishSignal, StoppedSignal, _referenceToActionsQueue));
        }
    }

    public void Start()
    {
        _startSemaphore.Release();
    }

    public void Join()
    {
        // This is not the same as Join() from standard thread: first of all it MUST be called each time (otherwise thread is not returned to the pool).
        FinishSignal.Wait();
        InternalJoin();
    }

    internal void InternalJoin()
    {
        threads.Push((_thread, _startSemaphore, FinishSignal, StoppedSignal, _referenceToActionsQueue));
    }

    public static void Destroy()
    {
        // Not a standard Dispose because this method removes static objects and should only be called when optimizers will no longer be needed.
        cts.Cancel();
        while (threads.TryPop(out var t))
        {
            t.Item4.Wait();
            t.Item4.Dispose();
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
    private readonly ConcurrentQueue<Action> _actions = new();

    public ParallelForEach(int maxThreads, List<T> items, Action<T> action)
    {
        var threadsNumber = Math.Min(maxThreads, items.Count);
        _threads = new ReusableThread[threadsNumber];
        for (var i = 0; i < items.Count; i++) EnqueueBeforeStart(GetForEachAction(action, items[i]));
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

    private static Action GetForEachAction(Action<T> action, T x)
    {
        return () => action(x);
    }

    public void Join()
    {
        for (var i = 0; i < _threads.Length; i++)
        {
            _threads[i].FinishSignal.Wait();
        }
       
        for (var i = 0; i < _threads.Length; i++)
        {
            _threads[i].InternalJoin();
        }
    }
}