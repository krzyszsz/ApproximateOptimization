using System;
using System.Threading;

namespace ApproximateOptimization
{
    /// <summary>
    /// All optimizers in this project work on local array with no memory allocations,
    /// therefore it can be efficient to run each of them in a separate thread
    /// (assuming that value function can also use constant memory).
    /// </summary>
    public class MultithreadedOptimizer<T> : IOptimizer, IOptimizerStats where T: new()
    {
        private MultiThreadedOptimizerParams<T> _problemParameters;

        public MultithreadedOptimizer(MultiThreadedOptimizerParams<T> problemParameters)
        {
            if (problemParameters == null)
            {
                throw new ArgumentNullException(nameof(problemParameters));
            }
            problemParameters.Validate();
            this._problemParameters = problemParameters;
        }

        public double[] BestSolutionSoFar { get; private set; }

        public double SolutionValue { get; private set; }

        public bool SolutionFound { get; private set; }

        public TimeSpan ElapsedTime { get; private set; }

        public long IterationsExecuted { get; private set; }

        public double LocalAreaAtTheEnd { get; private set; }

        public void FindMaximum()
        {
            var threads = new Thread[_problemParameters.ThreadCount];
            var optimizers = new IOptimizer[_problemParameters.ThreadCount];
            double[][] solutions = new double[_problemParameters.ThreadCount][];
            var lockSyncObject = new object();

            for (int i=0; i< _problemParameters.ThreadCount; i++)
            {
                var thread = new Thread((object threadId) => {
                    int threadIdInt = (int)threadId;
                    IOptimizer optimizer;
                    try
                    {
                        optimizer = _problemParameters.CreateOptimizer(threadIdInt);
                        optimizers[threadIdInt] = optimizer;
                    }
                    catch (Exception e)
                    {
                        _problemParameters.Logger.Error($"Error while creating an optimizer in thread ${threadId}: ${e}");
                        return;
                    }

                    try
                    {
                        optimizer.FindMaximum();
                        var optimizerStats = optimizer as IOptimizerStats;
                        if (optimizerStats != null)
                            lock (lockSyncObject)
                            {
                                ElapsedTime = optimizerStats.ElapsedTime; // From the last optimizer, not necessarily best
                                LocalAreaAtTheEnd = optimizerStats.LocalAreaAtTheEnd; // From the last optimizer, not necessarily best
                                IterationsExecuted += optimizerStats.IterationsExecuted;
                            }
                    }
                    catch (Exception e)
                    {
                       _problemParameters.Logger.Error($"Error while running an optimizer in thread ${threadId}: ${e}");
                    }
                });
                threads[i] = thread;
                thread.IsBackground = true;
                thread.Start(i);
            }

            for (int i=0; i< _problemParameters.ThreadCount; i++)
            {
                threads[i].Join();
            }

            for (int i=0; i< _problemParameters.ThreadCount; i++)
            {
                if ((optimizers[i]?.SolutionFound ?? false) &&
                    (!SolutionFound || SolutionValue < optimizers[i].SolutionValue))
                {
                    SolutionValue = optimizers[i].SolutionValue;
                    BestSolutionSoFar = optimizers[i].BestSolutionSoFar;
                    SolutionFound = true;
                }
            }
        }
    }
}
