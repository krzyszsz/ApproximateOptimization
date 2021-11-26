using System;

namespace ApproximateOptimization
{
    /// <summary>
    /// This optimizer employs MultithreadedOptimizer to run SimulatedAnnealingWithLocalAreaBinarySearch.
    /// </summary>
    public class CompositeOptimizer : MultithreadedOptimizer
    {
        public CompositeOptimizer(int threadCount = 8, ILogger logger = null, double temperatureMultiplier = 0.99,
            double localAreaMultiplier = 0.2, int iterationCount = 3, int iterationsPerDimension = 10)
            : base(threadId => new SimulatedAnnealingWithLocalAreaBinarySearch(
                    temperatureMultiplier,
                    threadId,
                    false,
                    true,
                    localAreaMultiplier,
                    iterationCount,
                    iterationsPerDimension),
                 threadCount,
                 logger)
        {
        }
    }
}
