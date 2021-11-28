namespace ApproximateOptimization
{
    public class CompositeOptimizer<T, P> : MultithreadedOptimizer<T>
        where T : MultiThreadedOptimizerParams<P>, new()
        where P : BaseSolutionFinderParams, new()
    {
        public CompositeOptimizer(CompositeOptimizerParams<T> problemParams)
            : base(problemParams)
        {
        }
    }

    /// <summary>
    /// This optimizer runs in multiple threads the optimizer built as symulated annealing with local
    /// area narrowing by gradient ascent search with local binary search.
    /// 
    /// This is concrete version of CompositeOptimizer to be used for convenience where multiple generic arguments
    /// could be confusing.
    /// </summary>
    public class NonGenericCompositeOptimizer : CompositeOptimizer<MultiThreadedOptimizerParams<SimulatedAnnealingWithLocalAreaBinarySearchParams>, SimulatedAnnealingWithLocalAreaBinarySearchParams>
    {
        public NonGenericCompositeOptimizer(CompositeOptimizerParams<MultiThreadedOptimizerParams<SimulatedAnnealingWithLocalAreaBinarySearchParams>> problemParams) : base(problemParams)
        {
        }
    }
}
