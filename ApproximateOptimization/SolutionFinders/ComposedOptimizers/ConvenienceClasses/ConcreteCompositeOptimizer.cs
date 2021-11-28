namespace ApproximateOptimization
{
    /// <summary>
    /// This optimizer runs in multiple threads the optimizer built as symulated annealing with local
    /// area narrowing by gradient ascent search with local binary search.
    /// 
    /// This is a concrete version of CompositeOptimizer to be used for convenience where multiple generic arguments
    /// could be confusing.
    /// </summary>
    public class ConcreteCompositeOptimizer : CompositeOptimizer<ConcreteMuiltiThreadedOptimizerParams, SimulatedAnnealingWithLocalAreaBinarySearchParams>
    {
        public ConcreteCompositeOptimizer(ConcreteMuiltiThreadedOptimizerParams problemParams)
            : base(problemParams)
        {
        }
    }
}
