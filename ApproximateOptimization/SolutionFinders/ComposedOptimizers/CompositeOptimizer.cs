namespace ApproximateOptimization
{
    public class CompositeOptimizer<T, P> : MultithreadedOptimizer<P>
        where T : MultiThreadedOptimizerParams<P>, new()
        where P : BaseSolutionFinderParams, new()
    {
        public CompositeOptimizer(T problemParams)
            : base(problemParams)
        {
        }
    }
}
