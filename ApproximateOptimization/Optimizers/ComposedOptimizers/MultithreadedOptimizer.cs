using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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
        private object _lockSyncObject = new object();
        private IOptimizer[] _optimizers;

        public MultithreadedOptimizer(MultiThreadedOptimizerParams<T> problemParameters)
        {
            if (problemParameters == null)
            {
                throw new ArgumentNullException(nameof(problemParameters));
            }
            problemParameters.Validate();
            _problemParameters = problemParameters;
            _bestSolutionsForGA = new PriorityQueue<double[], double>(_problemParameters.GAPopulation+1);
            _random = new Random(_problemParameters.NonRepeatableRandom ? (int)DateTime.UtcNow.Ticks : 0);
        }

        public double[] BestSolutionSoFar { get; private set; }

        public double SolutionValue { get; private set; }

        public bool SolutionFound { get; private set; }

        public TimeSpan ElapsedTime { get; private set; }

        public long IterationsExecuted { get; private set; }

        public double LocalAreaAtTheEnd { get; private set; }

        private ConcurrentDictionary<TabooSearchItem, double> _cacheForTabooSearch;

        public void FindMaximum()
        {
            if (SolutionFound) throw new ApplicationException("Cannot call FindMaximum twice on the same instance of optimizer.");
            int unallocatedProblemPartitions = _problemParameters.Partitions ?? _problemParameters.ThreadCount;
            var threads = new ReusableThread[_problemParameters.ThreadCount];
            _optimizers = new IOptimizer[unallocatedProblemPartitions];
            double[][] solutions = new double[unallocatedProblemPartitions][];

            if (_problemParameters.TabooSearch)
            {
                _cacheForTabooSearch = new ConcurrentDictionary<TabooSearchItem, double>();
                var originalFunc = _problemParameters.ScoreFunction;
                _problemParameters.ScoreFunction = (solution) =>
                {
                    var reducedSolution = new TabooSearchItem(solution, _problemParameters.TabooAreaForAllDimensions);
                    if (_cacheForTabooSearch.ContainsKey(reducedSolution))
                    {
                        return double.MinValue;
                    }
                    var result = originalFunc(solution);
                    _cacheForTabooSearch[reducedSolution] = result;
                    return result;
                };
            }

            var customThreadPool = new ParallelForEach<int>(_problemParameters.ThreadCount, Enumerable.Range(0, unallocatedProblemPartitions).ToList(), RunSinglePartition);
            customThreadPool.Join();

            for (int i=0; i< _optimizers.Length; i++)
            {
                if (_optimizers[i]?.SolutionFound ?? false)
                {
                    _bestSolutionsForGA.Enqueue(_optimizers[i].BestSolutionSoFar, _optimizers[i].SolutionValue);
                    if (_bestSolutionsForGA.Count > _problemParameters.GAPopulation)
                    {
                        _bestSolutionsForGA.Dequeue();
                    }
                }
            }

            var syncRoot = new object();
            for (var i=0; i<_problemParameters.GAGenerations; i++)
            {
                RunGA((sol, val) =>
                {
                    lock (syncRoot)
                    {
                        _bestSolutionsForGA.Enqueue(sol, val.Value);
                        if (_bestSolutionsForGA.Count > _problemParameters.GAPopulation)
                        {
                            _bestSolutionsForGA.Dequeue();
                        }
                    }
                });
            }

            while (_bestSolutionsForGA.TryDequeue(out var solution, out var value))
            {
                BestSolutionSoFar = solution;
                SolutionValue = value;
                SolutionFound = true;
            }
        }

        private void RunSinglePartition(int partitionId)
        {
            IOptimizer optimizer;
            try
            {
                optimizer = _problemParameters.CreateOptimizer(partitionId);
                var simulatedAnnealingOptimizer = optimizer as SimulatedAnnealingOptimizer;
                if (simulatedAnnealingOptimizer != null)
                {
                    simulatedAnnealingOptimizer.SetInitialStage(((double)partitionId) / _problemParameters.ThreadCount);
                }
                _optimizers[partitionId] = optimizer;
            }
            catch (Exception e)
            {
                _problemParameters.Logger.Error($"Error while creating an optimizer in partition ${partitionId}: ${e}");
                return;
            }

            try
            {
                optimizer.FindMaximum();
                var optimizerStats = optimizer as IOptimizerStats;
                if (optimizerStats != null)
                    lock (_lockSyncObject)
                    {
                        ElapsedTime = optimizerStats.ElapsedTime; // From the last optimizer, not necessarily best
                        LocalAreaAtTheEnd = optimizerStats.LocalAreaAtTheEnd; // From the last optimizer, not necessarily best
                        IterationsExecuted += optimizerStats.IterationsExecuted;
                    }
            }
            catch (Exception e)
            {
                _problemParameters.Logger.Error($"Error while running an optimizer in partition ${partitionId}: ${e}");
            }
        }
        private void RunGA(Action<double[], double?> nextSolutionSuggestedCallback)
        {
            var items = new List<double[]>();
            foreach (var item in _bestSolutionsForGA.UnorderedItems)
            {
                items.Add(item.Element);
            }
            if (items.Count < 2) return;
            var solutionsToCheck = new List<double[]>();
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                for (var j = 0; j < _problemParameters.GAChildrenPerSolution; j++)
                {
                    var otherElementIdx = _random.Next(items.Count - 1);
                    if (otherElementIdx >= i) otherElementIdx++;
                    var newSolutionToCheck = CrossOver(item, items[otherElementIdx]);
                    solutionsToCheck.Add(newSolutionToCheck);
                }
            }
            var customThreadPool = new ParallelForEach<double[]>(_problemParameters.ThreadCount, solutionsToCheck, newSolutionToCheck => nextSolutionSuggestedCallback(newSolutionToCheck, _problemParameters.ScoreFunction(newSolutionToCheck)));
            customThreadPool.Join();
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

        private class TabooSearchItem : IEquatable<TabooSearchItem>
        {
            private double[] _reducedPoint;

            public TabooSearchItem(double[] originalPoint, double localAreaSize)
            {
                _reducedPoint = new double[originalPoint.Length];
                for (var i = 0; i < originalPoint.Length; i++)
                {
                    _reducedPoint[i] = ((int)(originalPoint[i] / localAreaSize)) * localAreaSize;
                }
            }

            public bool Equals(TabooSearchItem other)
            {
                for (var i = 0; i < other._reducedPoint.Length; i++)
                {
                    if (_reducedPoint[i] != other._reducedPoint[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                TabooSearchItem otherObj = obj as TabooSearchItem;
                if (otherObj == null)
                    return false;
                return Equals(otherObj);
            }

            public override int GetHashCode()
            {
                int result = 0;
                for (var i = 0; i < _reducedPoint.Length; i++)
                {
                    result = result ^ _reducedPoint[i].GetHashCode();
                }
                return result;
            }
        }
    }
}
