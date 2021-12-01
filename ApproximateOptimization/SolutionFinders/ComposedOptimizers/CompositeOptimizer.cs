namespace ApproximateOptimization
{
    public class CompositeOptimizer<T, P> : MultithreadedOptimizer<P>
        where T : MultiThreadedOptimizerParams<P>
        where P : BaseOptimizerParams, new()
    {
        public CompositeOptimizer(T problemParams)
            : base(problemParams)
        {
        }
    }
}
