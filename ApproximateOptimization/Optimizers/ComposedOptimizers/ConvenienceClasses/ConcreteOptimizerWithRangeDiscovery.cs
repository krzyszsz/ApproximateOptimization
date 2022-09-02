namespace ApproximateOptimization
{
    public class ConcreteOptimizerWithRangeDiscovery : AutoTuningFinder<
                ConcreteOptimizerWithRangeDiscoveryParams, MultiStrategyOptimizerParams>
    {
        public ConcreteOptimizerWithRangeDiscovery(ConcreteOptimizerWithRangeDiscoveryParams problemParameters) : base(problemParameters)
        {
        }
    }
}