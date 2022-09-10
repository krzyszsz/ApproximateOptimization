﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
            int unallocatedProblemPartitions = _problemParameters.Partitions ?? _problemParameters.ThreadCount;
            var threads = new Thread[_problemParameters.ThreadCount];
            _optimizers = new IOptimizer[unallocatedProblemPartitions];
            double[][] solutions = new double[unallocatedProblemPartitions][];
            long partitionId = 0;

            for (int i=0; i< _problemParameters.ThreadCount; i++)
            {
                var thread = new Thread(() =>
                {
                    while (UnallocatedPartitionsStillExist(ref unallocatedProblemPartitions))
                    {
                        long partitionIdLocal;
                        lock (_lockSyncObject)
                        {
                            partitionIdLocal = partitionId++;
                        }
                        RunSinglePartition(partitionIdLocal);
                    }
                });
                threads[i] = thread;
                thread.IsBackground = true;
                thread.Start();
            }

            for (int i=0; i< _problemParameters.ThreadCount; i++)
            {
                threads[i].Join();
            }

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

        private void RunSinglePartition(long partitionId)
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
                _problemParameters.Logger.Error($"Error while running an optimizer in thread ${partitionId}: ${e}");
            }
        }

        private bool UnallocatedPartitionsStillExist(ref int unallocatedProblemPartitions)
        {
            bool unallocatedPartitionsExist;
            lock (_lockSyncObject)
            {
                unallocatedPartitionsExist = unallocatedProblemPartitions > 0;
                if (unallocatedPartitionsExist) unallocatedProblemPartitions--;
            }

            return unallocatedPartitionsExist;
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
            Parallel.ForEach(solutionsToCheck, 
                new ParallelOptions { MaxDegreeOfParallelism = _problemParameters.ThreadCount },
                newSolutionToCheck => nextSolutionSuggestedCallback(newSolutionToCheck, _problemParameters.ScoreFunction(newSolutionToCheck)));
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
