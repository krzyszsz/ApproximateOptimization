using System;
using System.Threading;

namespace ApproximateOptimization
{
    /// <summary>
    /// All solution finders in this project work on local array with no memory allocations,
    /// therefore it can be efficient to run each of them in a separate thread
    /// (assuming that value function can also use constant memory).
    /// </summary>
    public class MultithreadedOptimizer<T> : ISolutionFinder<T> where T: BaseSolutionFinderParams, new()
    {
        private MultiThreadedOptimizerParams<T> problemParameters;

        public MultithreadedOptimizer(MultiThreadedOptimizerParams<T> problemParameters)
        {
            BaseSolutionFinder<MultiThreadedOptimizerParams<T>>.ProcessParameters(problemParameters);
            this.problemParameters = problemParameters;
        }

        public double[] BestSolutionSoFar { get; private set; }

        public double SolutionValue { get; private set; }

        public bool SolutionFound { get; private set; }

        public void FindMaximum()
        {
            var threads = new Thread[problemParameters.threadCount];
            var solutionFinders = new ISolutionFinder<T>[problemParameters.threadCount];
            double[][] solutions = new double[problemParameters.threadCount][];

            for (int i=0; i< problemParameters.threadCount; i++)
            {
                var thread = new Thread((object threadId) => {
                    int threadIdInt = (int)threadId;
                    ISolutionFinder<T> solutionFinder;
                    try
                    {
                        solutionFinder = problemParameters.createSolutionFinder(threadIdInt);
                        solutionFinders[threadIdInt] = solutionFinder;
                    }
                    catch (Exception e)
                    {
                        problemParameters.logger.Error($"Error while creating solution finder in thread ${threadId}: ${e}");
                        return;
                    }

                    try
                    {
                        solutionFinder.FindMaximum();
                    }
                    catch (Exception e)
                    {
                        problemParameters.logger.Error($"Error while running solution finder in thread ${threadId}: ${e}");
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
