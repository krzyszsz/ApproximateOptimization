using System;
using System.Threading;

namespace ApproximateOptimization
{
    /// <summary>
    /// All solution finders in this project work on local array with no memory allocations,
    /// therefore it can be efficient to run each of them in a separate thread
    /// (assuming that value function can also use const memory).
    /// </summary>
    public class MultithreadedOptimizer : ISolutionFinder
    {
        private ILogger logger;
        private int threadCount;
        private Func<int, ISolutionFinder> createSolutionFinder;

        public MultithreadedOptimizer(Func<int, ISolutionFinder> createSolutionFinder, int threadCount = 8, ILogger logger = null)
        {
            this.logger = logger ?? ThreadSafeConsoleLogger.Instance;
            this.createSolutionFinder = createSolutionFinder;
            this.threadCount = threadCount;
        }

        public double[] BestSolutionSoFar { get; private set; }

        public double SolutionValue { get; private set; }

        public bool SolutionFound { get; private set; }

        public void FindMaximum(int dimension, Func<double[], double> getValue, TimeSpan timeLimit = default, long maxIterations = -1)
        {
            var threads = new Thread[threadCount];
            var solutionFinders = new ISolutionFinder[threadCount];
            double[][] solutions = new double[threadCount][];

            for (int i=0; i<threadCount; i++)
            {
                var thread = new Thread((object threadId) => {
                    int threadIdInt = (int)threadId;
                    ISolutionFinder solutionFinder;
                    try
                    {
                        solutionFinder = createSolutionFinder(threadIdInt);
                        solutionFinders[threadIdInt] = solutionFinder;
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Error while creating solution finder in thread ${threadId}: ${e}");
                        return;
                    }

                    try
                    {
                        solutionFinder.FindMaximum(dimension, getValue, timeLimit, maxIterations);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Error while running solution finder in thread ${threadId}: ${e}");
                    }
                });
                threads[i] = thread;
                thread.IsBackground = true;
                thread.Start(i);
            }

            for (int i=0; i<threadCount; i++)
            {
                threads[i].Join();
            }

            for (int i=0; i<threadCount; i++)
            {
                if ((solutionFinders[i]?.SolutionFound ?? false) &&
                    (!SolutionFound || SolutionValue < solutionFinders[i].SolutionValue))
                {
                    SolutionValue = solutionFinders[i].SolutionValue;
                    BestSolutionSoFar = solutionFinders[i].BestSolutionSoFar;
                    SolutionFound = true;
                }
            }
        }
    }
}
