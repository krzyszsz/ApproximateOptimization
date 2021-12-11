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
        private MultiThreadedOptimizerParams<T> problemParameters;

        public MultithreadedOptimizer(MultiThreadedOptimizerParams<T> problemParameters)
        {
            if (problemParameters == null)
            {
                throw new ArgumentNullException(nameof(problemParameters));
            }
            problemParameters.Validate();
            this.problemParameters = problemParameters;
        }

        public double[] BestSolutionSoFar { get; private set; }

        public double SolutionValue { get; private set; }

        public bool SolutionFound { get; private set; }

        public TimeSpan ElapsedTime { get; private set; }

        public long IterationsExecuted { get; private set; }

        public double LocalAreaAtTheEnd { get; private set; }

        public void FindMaximum()
        {
            var threads = new Thread[problemParameters.threadCount];
            var optimizers = new IOptimizer[problemParameters.threadCount];
            double[][] solutions = new double[problemParameters.threadCount][];
            var lockSyncObject = new object();

            for (int i=0; i< problemParameters.threadCount; i++)
            {
                var thread = new Thread((object threadId) => {
                    int threadIdInt = (int)threadId;
                    IOptimizer optimizer;
                    try
                    {
                        optimizer = problemParameters.createOptimizer(threadIdInt);
                        optimizers[threadIdInt] = optimizer;
                    }
                    catch (Exception e)
                    {
                        problemParameters.logger.Error($"Error while creating an optimizer in thread ${threadId}: ${e}");
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
                        problemParameters.logger.Error($"Error while running an optimizer in thread ${threadId}: ${e}");
                    }
                });
                threads[i] = thread;
                thread.IsBackground = true;
                thread.Start(i);
            }

            for (int i=0; i< problemParameters.threadCount; i++)
            {
                threads[i].Join();
            }

            for (int i=0; i< problemParameters.threadCount; i++)
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
