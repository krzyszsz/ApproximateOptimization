using System;
using System.Collections.Generic;
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
        private PriorityQueue<double[], double> _bestSolutionsForGA;
        protected readonly Random _random;

        public MultithreadedOptimizer(MultiThreadedOptimizerParams<T> problemParameters)
        {
            if (problemParameters == null)
            {
                throw new ArgumentNullException(nameof(problemParameters));
            }
            problemParameters.Validate();
            _problemParameters = problemParameters;
            _bestSolutionsForGA = new PriorityQueue<double[], double>(_problemParameters.GAPopulation+1);
            _random = new Random(0);
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
                        var simulatedAnnealingOptimizer = optimizer as SimulatedAnnealingOptimizer;
                        if (simulatedAnnealingOptimizer != null)
                        {
                            simulatedAnnealingOptimizer.SetInitialStage(((double)i) / _problemParameters.ThreadCount);
                        }
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
                if (optimizers[i]?.SolutionFound ?? false)
                {
                    _bestSolutionsForGA.Enqueue(optimizers[i].BestSolutionSoFar, optimizers[i].SolutionValue);
                    if (_bestSolutionsForGA.Count > _problemParameters.GAPopulation)
                    {
                        _bestSolutionsForGA.Dequeue();
                    }
                }
            }

            RunGA((sol, val) =>
            {
                _bestSolutionsForGA.Enqueue(sol, val.Value);
                if (_bestSolutionsForGA.Count > _problemParameters.GAPopulation)
                {
                    _bestSolutionsForGA.Dequeue();
                }
            });

            while (_bestSolutionsForGA.TryDequeue(out var solution, out var value))
            {
                BestSolutionSoFar = solution;
                SolutionValue = value;
                SolutionFound = true;
            }
        }

        private void RunGA(Action<double[], double?> nextSolutionSuggestedCallback)
        {
            var items = new List<double[]>();
            foreach (var item in _bestSolutionsForGA.UnorderedItems)
            {
                items.Add(item.Element);
            }
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                for (var j = 0; j < _problemParameters.GAChildrenPerSolution; j++)
                {
                    if (items.Count < 2) break;
                    var otherElementIdx = _random.Next(items.Count - 1);
                    if (otherElementIdx >= i) otherElementIdx++;
                    var newSolutionToCheck = CrossOver(item, items[otherElementIdx]);
                    nextSolutionSuggestedCallback(newSolutionToCheck, _problemParameters.ScoreFunction(newSolutionToCheck));
                }
            }
        }

        private double[] CrossOver(double[] item1, double[] item2)
        {
            var result = new double[item1.Length];
            for (var i = 0; i < item1.Length; i++)
            {
                var weight = _random.NextDouble();
                result[i] = weight * item1[i] + (1 - weight) * item2[i];
            }
            return result;
        }
    }
}
