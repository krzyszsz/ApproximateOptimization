namespace ApproximateOptimization
{
    public class CompositeOptimizer<T, P> : MultithreadedOptimizer<P>
        where T : MultiThreadedOptimizerParams<P>
        where P : BaseSolutionFinderParams, new()
    {
        public CompositeOptimizer(T problemParams)
            : base(problemParams)
        {
        }
    }
}
