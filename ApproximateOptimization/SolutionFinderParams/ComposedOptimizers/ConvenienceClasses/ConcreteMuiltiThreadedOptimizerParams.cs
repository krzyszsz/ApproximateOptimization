namespace ApproximateOptimization
{
    /// <summary>
    /// Convenience class with all generic arguments provided.
    /// </summary>
    public class ConcreteMuiltiThreadedOptimizerParams : MultiThreadedOptimizerParams<SimulatedAnnealingWithLocalAreaBinarySearchParams>
    {
        public ConcreteMuiltiThreadedOptimizerParams()
        {
            createSolutionFinder = (int threadNumber) => new SimulatedAnnealingWithLocalAreaBinarySearch<SimulatedAnnealingWithLocalAreaBinarySearchParams>(actualOptimizerParams);
        }
    }
}
