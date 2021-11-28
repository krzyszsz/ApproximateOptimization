namespace ApproximateOptimization
{
    /// <summary>
    /// This optimizer employs MultithreadedOptimizer to run SimulatedAnnealingWithLocalAreaBinarySearch.
    /// </summary>
    public class CompositeOptimizer<T, P> : MultithreadedOptimizer<T>
        where T : MultiThreadedOptimizerParams<P>, new()
        where P : BaseSolutionFinderParams
    {
        public CompositeOptimizer(CompositeOptimizerParams<T> problemParams)
            : base(problemParams)
        {
            var q = (MultiThreadedOptimizerParams<T>)problemParams;
        }
    }
}
