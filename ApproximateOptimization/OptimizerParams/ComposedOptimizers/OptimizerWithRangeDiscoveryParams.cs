using System;

namespace ApproximateOptimization
{
    public class OptimizerWithRangeDiscoveryParams<T> : BaseOptimizerParams where T : BaseOptimizerParams
    {
        public Func<double[][], IOptimizer> OptimizerFactoryMethod { get; set; }
        public int MaxAttempts { get; set; } = 50;
    }
}
