using System;

namespace ApproximateOptimization
{
    public class CompositeOptimizerParams<T> : MultiThreadedOptimizerParams<T> where T : BaseSolutionFinderParams, new()
    { }
}
