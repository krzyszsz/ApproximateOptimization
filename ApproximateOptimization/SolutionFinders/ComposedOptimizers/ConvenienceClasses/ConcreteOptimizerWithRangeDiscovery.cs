namespace ApproximateOptimization
{
    public class ConcreteOptimizerWithRangeDiscovery : AutoTuningFinder<
                ConcreteOptimizerWithRangeDiscoveryParams, SimulatedAnnealingWithGradientAscentOptimizerParams>
    {
        public ConcreteOptimizerWithRangeDiscovery(ConcreteOptimizerWithRangeDiscoveryParams problemParameters) : base(problemParameters)
        {
        }
    }
}