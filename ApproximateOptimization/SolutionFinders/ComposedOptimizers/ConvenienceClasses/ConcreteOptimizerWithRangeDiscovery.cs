namespace ApproximateOptimization
{
    public class ConcreteOptimizerWithRangeDiscovery : OptimizerWithRangeDiscovery<
                ConcreteOptimizerWithRangeDiscoveryParams, SimulatedAnnealingWithGradientAscentOptimizerParams>
    {
        public ConcreteOptimizerWithRangeDiscovery(ConcreteOptimizerWithRangeDiscoveryParams problemParameters) : base(problemParameters)
        {
        }
    }
}