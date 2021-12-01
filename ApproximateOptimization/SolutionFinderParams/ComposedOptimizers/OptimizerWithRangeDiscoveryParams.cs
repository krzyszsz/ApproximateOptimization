using System;

namespace ApproximateOptimization
{
    public class OptimizerWithRangeDiscoveryParams<T> : BaseOptimizerParams where T : BaseOptimizerParams
    {
        public Func<double[][], IOptimizer> optimizerFactoryMethod { get; set; }
        public int maxAttempts { get; set; } = 50;
    }
}
