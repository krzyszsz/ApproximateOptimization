using System;

namespace ApproximateOptimization
{
    public class CompositeOptimizerParams<T> : MultiThreadedOptimizerParams<T> where T : BaseOptimizerParams, new()
    { }
}
