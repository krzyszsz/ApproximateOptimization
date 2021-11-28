using System;

namespace ApproximateOptimization
{
    public class MultiThreadedOptimizerParams<T> : BaseSolutionFinderParams where T: BaseSolutionFinderParams, new()
    {
        public T actualOptimizerParams { get; set; } = new T();
        public Func<int, ISolutionFinder<T>> createSolutionFinder { get; set; }
        public int threadCount { get; set; } = 8;
        public ILogger logger { get; set; } = ThreadSafeConsoleLogger.Instance;

        public override void Validate()
        {
            base.Validate();
            if (createSolutionFinder == null)
            {
                throw new ArgumentException("Missing createSolutionFinder argument.");
            }
            if (actualOptimizerParams == null)
            {
                throw new ArgumentException("Missing actualOptimizerParams argument.");
            }
            if (threadCount < 1)
            {
                throw new ArgumentException("Thread count argument should be at least 1.");
            }
            if (logger == null)
            {
                throw new ArgumentException("Missing logger argument.");
            }
        }
    }

    /// <summary>
    /// Convenience class with all generic arguments provided.
    /// </summary>
    public class NonGenericMuiltiThreadedOptimizerParams : MultiThreadedOptimizerParams<SimulatedAnnealingWithLocalAreaBinarySearchParams>
    {
        public NonGenericMuiltiThreadedOptimizerParams()
        {
            createSolutionFinder = (int threadNumber) => new SimulatedAnnealingWithLocalAreaBinarySearch<SimulatedAnnealingWithLocalAreaBinarySearchParams>(actualOptimizerParams);
        }
    }
}
